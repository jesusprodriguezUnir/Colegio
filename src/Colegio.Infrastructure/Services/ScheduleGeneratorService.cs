using Colegio.Domain.Entities;
using Colegio.Domain.Services;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ValidationResult = Colegio.Domain.Services.ValidationResult;

namespace Colegio.Infrastructure.Services;

public class ScheduleGeneratorService : IScheduleGenerator
{
    private readonly ColegioDbContext _context;

    public ScheduleGeneratorService(ColegioDbContext context)
    {
        _context = context;
    }

    public async Task<GenerationResult> GenerateAsync(Guid classroomId, AcademicSessionType sessionType)
    {
        var allSchedulesAtSession = await _context.Schedules
            .Include(s => s.TimeSlot)
            .Where(s => s.TimeSlot.SessionType == sessionType)
            .ToListAsync();

        var constraints = await _context.ScheduleConstraints
            .Where(c => c.IsActive)
            .ToListAsync();

        var rooms = await _context.Rooms.ToListAsync();

        try
        {
            var schedules = await GenerateForClassroomInternalAsync(classroomId, sessionType, allSchedulesAtSession, constraints, rooms);
            var score = CalculateScore(schedules, constraints);
            return new GenerationResult(schedules, score, new List<string>(), true);
        }
        catch (Exception ex)
        {
            return new GenerationResult(new List<Schedule>(), 0, new List<string> { ex.Message }, false);
        }
    }

    public async Task<GenerationResult> GenerateAllAsync(AcademicSessionType sessionType)
    {
        var classrooms = await _context.Classrooms.ToListAsync();
        var allSchedulesAtSession = await _context.Schedules
            .Include(s => s.TimeSlot)
            .Where(s => s.TimeSlot.SessionType == sessionType)
            .ToListAsync();

        var constraints = await _context.ScheduleConstraints.Where(c => c.IsActive).ToListAsync();
        var rooms = await _context.Rooms.ToListAsync();
        var allNewSchedules = new List<Schedule>();
        var warnings = new List<string>();

        foreach (var classroom in classrooms)
        {
            try
            {
                var classroomSchedules = await GenerateForClassroomInternalAsync(
                    classroom.Id, sessionType, allSchedulesAtSession, constraints, rooms);

                var newOnly = classroomSchedules
                    .Where(s => !allSchedulesAtSession.Any(existing => existing.Id == s.Id))
                    .ToList();
                allSchedulesAtSession.AddRange(newOnly);
                allNewSchedules.AddRange(newOnly);
            }
            catch (Exception ex)
            {
                warnings.Add($"{classroom.GradeLevel}º {classroom.Line}: {ex.Message}");
            }
        }

        var score = CalculateScore(allNewSchedules, constraints);
        return new GenerationResult(allNewSchedules, score, warnings, warnings.Count == 0);
    }

    public async Task<ValidationResult> ValidateAsync(AcademicSessionType sessionType)
    {
        var schedules = await _context.Schedules
            .Include(s => s.TimeSlot)
            .Include(s => s.Teacher)
            .Include(s => s.Room)
            .Where(s => s.TimeSlot.SessionType == sessionType)
            .ToListAsync();

        var conflicts = new List<ConflictInfo>();

        // Check teacher conflicts (same teacher, same timeslot)
        var teacherGroups = schedules.GroupBy(s => new { s.TeacherId, s.TimeSlotId });
        foreach (var group in teacherGroups.Where(g => g.Count() > 1))
        {
            conflicts.Add(new ConflictInfo(
                "TeacherConflict",
                $"Profesor asignado a {group.Count()} clases simultáneas",
                group.Key.TeacherId, null, group.Key.TimeSlotId));
        }

        // Check room conflicts (same room, same timeslot)
        var roomGroups = schedules.Where(s => s.RoomId != null).GroupBy(s => new { s.RoomId, s.TimeSlotId });
        foreach (var group in roomGroups.Where(g => g.Count() > 1))
        {
            conflicts.Add(new ConflictInfo(
                "RoomConflict",
                $"Aula asignada a {group.Count()} clases simultáneas",
                null, group.Key.RoomId, group.Key.TimeSlotId));
        }

        // Check curriculum gaps
        var classroomGroups = schedules.GroupBy(s => s.ClassroomId);
        var classrooms = await _context.Classrooms.ToListAsync();
        foreach (var cGroup in classroomGroups)
        {
            var classroom = classrooms.FirstOrDefault(c => c.Id == cGroup.Key);
            if (classroom == null) continue;

            var curriculum = await _context.Curriculums
                .Where(c => c.GradeLevel == classroom.GradeLevel)
                .ToListAsync();

            foreach (var curr in curriculum)
            {
                var assignedHours = cGroup.Count(s => s.SubjectId == curr.SubjectId);
                if (assignedHours < curr.WeeklyHours)
                {
                    conflicts.Add(new ConflictInfo(
                        "CurriculumGap",
                        $"Faltan {curr.WeeklyHours - assignedHours}h semanales",
                        null, null, null));
                }
            }
        }

        return new ValidationResult(conflicts.Count == 0, conflicts);
    }

    public async Task<DebugResult> DebugConstraintsAsync(Guid classroomId, AcademicSessionType sessionType)
    {
        var impossible = new List<string>();
        var suggestions = new List<string>();

        var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classroomId);
        if (classroom == null) return new DebugResult(new List<string> { "Aula no encontrada" }, suggestions, 0, 0);

        var curriculum = await _context.Curriculums
            .Where(c => c.GradeLevel == classroom.GradeLevel)
            .Include(c => c.Subject)
            .ToListAsync();

        var timeSlots = await _context.TimeSlots
            .Where(ts => ts.SessionType == sessionType && !ts.IsBreak)
            .ToListAsync();

        var teachers = await _context.Teachers
            .Include(t => t.Subjects)
            .Include(t => t.Availabilities)
            .ToListAsync();

        int totalHoursNeeded = curriculum.Sum(c => c.WeeklyHours);
        int availableSlots = timeSlots.Count;

        if (totalHoursNeeded > availableSlots)
        {
            impossible.Add($"Se necesitan {totalHoursNeeded} horas pero solo hay {availableSlots} franjas disponibles");
            suggestions.Add("Reduce horas del currículo o añade más franjas horarias");
        }

        foreach (var curr in curriculum)
        {
            var qualifiedTeachers = teachers
                .Where(t => t.Subjects.Any(s => s.Id == curr.SubjectId))
                .ToList();

            if (qualifiedTeachers.Count == 0)
            {
                impossible.Add($"No hay profesores para '{curr.Subject.Name}'");
                suggestions.Add($"Asigna la competencia '{curr.Subject.Name}' a algún profesor");
            }

            var availableHours = qualifiedTeachers
                .SelectMany(t => t.Availabilities.Where(a => a.Level != AvailabilityLevel.Unavailable))
                .Select(a => a.TimeSlotId)
                .Distinct()
                .Count();

            if (availableHours < curr.WeeklyHours)
            {
                impossible.Add($"Profesores de '{curr.Subject.Name}' solo disponibles {availableHours}h, se necesitan {curr.WeeklyHours}h");
            }
        }

        var constraints = await _context.ScheduleConstraints.Where(c => c.IsActive).ToListAsync();
        return new DebugResult(impossible, suggestions, constraints.Count, constraints.Count - impossible.Count);
    }

    public async Task<ScheduleScore> CalculateScoreAsync(AcademicSessionType sessionType)
    {
        var schedules = await _context.Schedules
            .Include(s => s.TimeSlot)
            .Include(s => s.Teacher)
            .Where(s => s.TimeSlot.SessionType == sessionType)
            .ToListAsync();

        var constraints = await _context.ScheduleConstraints.Where(c => c.IsActive).ToListAsync();
        var availabilities = await _context.TeacherAvailabilities.ToListAsync();

        var details = new List<string>();

        // Teacher satisfaction: how well preferences are met
        double teacherSat = CalculateTeacherSatisfaction(schedules, availabilities, details);
        double compactness = CalculateCompactness(schedules, details);
        double balance = CalculateBalance(schedules, details);
        double roomOpt = 80.0; // Placeholder — will improve in Phase 2

        double total = (teacherSat * 0.3) + (compactness * 0.25) + (balance * 0.25) + (roomOpt * 0.2);

        return new ScheduleScore(Math.Round(total, 1), Math.Round(teacherSat, 1),
            Math.Round(compactness, 1), Math.Round(balance, 1), Math.Round(roomOpt, 1), details);
    }

    // ── Internal generation ──────────────────────────────────────────────

    private async Task<List<Schedule>> GenerateForClassroomInternalAsync(
        Guid classroomId, AcademicSessionType sessionType,
        List<Schedule> allSchedulesAtSession, List<ScheduleConstraint> constraints, List<Room> rooms)
    {
        var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classroomId)
            ?? throw new Exception("Classroom not found");

        var curriculum = await _context.Curriculums
            .Where(c => c.GradeLevel == classroom.GradeLevel)
            .Include(c => c.Subject).ThenInclude(s => s.RequiredRoom)
            .ToListAsync();

        var timeSlots = (await _context.TimeSlots
            .Where(ts => ts.SessionType == sessionType)
            .ToListAsync())
            .OrderBy(ts => ts.DayOfWeek).ThenBy(ts => ts.StartTime).ToList();

        var teachers = await _context.Teachers
            .Include(t => t.Subjects)
            .Include(t => t.Availabilities)
            .ToListAsync();

        // Build pending assignments sorted by constraint difficulty
        var pendingAssignments = new List<PendingAssignment>();
        foreach (var curr in curriculum)
        {
            for (int i = 0; i < curr.WeeklyHours; i++)
            {
                pendingAssignments.Add(new PendingAssignment
                {
                    SubjectId = curr.SubjectId,
                    SubjectName = curr.Subject.Name,
                    IsEf = curr.Subject.Name.Contains("Educación Física"),
                    RequiredRoomId = curr.Subject.RequiredRoomId
                });
            }
        }

        pendingAssignments = pendingAssignments
            .OrderByDescending(a => a.IsEf)
            .ThenBy(a => teachers.Count(t => t.Subjects.Any(s => s.Id == a.SubjectId)))
            .ThenByDescending(a => a.RequiredRoomId.HasValue)
            .ToList();

        // Grid and locked pre-assignment
        var existingForClassroom = allSchedulesAtSession.Where(s => s.ClassroomId == classroomId).ToList();
        var grid = new Dictionary<Guid, Schedule>();

        foreach (var s in existingForClassroom)
        {
            if (s.IsLocked)
            {
                grid[s.TimeSlotId] = s;
                var pending = pendingAssignments.FirstOrDefault(a => a.SubjectId == s.SubjectId);
                if (pending != null) pendingAssignments.Remove(pending);
            }
        }

        if (Solve(0, pendingAssignments, grid, timeSlots, teachers, allSchedulesAtSession, classroom, constraints, rooms))
        {
            return grid.Values.ToList();
        }

        throw new Exception($"No se encontró un horario válido para {classroom.GradeLevel}º {classroom.Line}");
    }

    private bool Solve(int index, List<PendingAssignment> pending, Dictionary<Guid, Schedule> grid,
        List<TimeSlot> slots, List<Teacher> teachers, List<Schedule> allSchedules,
        Classroom classroom, List<ScheduleConstraint> constraints, List<Room> rooms)
    {
        if (index >= pending.Count) return true;

        var current = pending[index];

        foreach (var slot in slots)
        {
            if (slot.IsBreak || grid.ContainsKey(slot.Id)) continue;

            // EF-specific rules
            if (current.IsEf && !IsEfSlotValid(classroom.GradeLevel, slot)) continue;
            if (!current.IsEf && IsEfSlotReservedForGrade(classroom.GradeLevel, slot)
                && pending.Skip(index).Any(a => a.IsEf)) continue;

            // Hard constraint: NonConsecutiveDays for same subject
            if (HasNonConsecutiveDayViolation(current, slot, grid, slots, constraints)) continue;

            // Hard constraint: MaxConsecutiveHours for teachers
            var candidateTeachers = teachers
                .Where(t => t.Subjects.Any(s => s.Id == current.SubjectId))
                .Where(t => t.Availabilities.Any(a => a.TimeSlotId == slot.Id
                    && a.Level != AvailabilityLevel.Unavailable))
                .Where(t => !allSchedules.Any(s => s.TeacherId == t.Id && s.TimeSlotId == slot.Id))
                .OrderByDescending(t => t.Availabilities
                    .FirstOrDefault(a => a.TimeSlotId == slot.Id)?.Level == AvailabilityLevel.Preferred)
                .ThenBy(t => allSchedules.Count(s => s.TeacherId == t.Id)) // Balance load
                .ToList();

            // Room assignment
            Guid? roomId = current.RequiredRoomId;
            if (roomId.HasValue)
            {
                bool roomBusy = allSchedules.Any(s => s.RoomId == roomId && s.TimeSlotId == slot.Id);
                if (roomBusy) continue;
            }

            foreach (var teacher in candidateTeachers)
            {
                // Check max daily hours constraint
                if (ExceedsMaxDailyHours(teacher, slot, allSchedules, grid, constraints)) continue;

                var newSchedule = new Schedule
                {
                    Id = Guid.NewGuid(),
                    ClassroomId = classroom.Id,
                    TeacherId = teacher.Id,
                    SubjectId = current.SubjectId,
                    TimeSlotId = slot.Id,
                    IsLocked = false,
                    Type = ScheduleType.ClassUnit,
                    RoomId = roomId
                };

                grid[slot.Id] = newSchedule;
                allSchedules.Add(newSchedule);

                if (Solve(index + 1, pending, grid, slots, teachers, allSchedules, classroom, constraints, rooms))
                    return true;

                grid.Remove(slot.Id);
                allSchedules.Remove(newSchedule);
            }
        }

        return false;
    }

    // ── Constraint checks ────────────────────────────────────────────────

    private bool HasNonConsecutiveDayViolation(PendingAssignment current, TimeSlot slot,
        Dictionary<Guid, Schedule> grid, List<TimeSlot> slots, List<ScheduleConstraint> constraints)
    {
        var constraint = constraints.FirstOrDefault(c =>
            c.Type == ConstraintType.NonConsecutiveDays && c.Hardness == ConstraintHardness.Hard
            && (c.SubjectId == null || c.SubjectId == current.SubjectId));

        if (constraint == null) return false;

        var adjacentDays = new[] { slot.DayOfWeek - 1, slot.DayOfWeek + 1 };
        foreach (var entry in grid)
        {
            if (entry.Value.SubjectId != current.SubjectId) continue;
            var entrySlot = slots.FirstOrDefault(s => s.Id == entry.Key);
            if (entrySlot != null && adjacentDays.Contains(entrySlot.DayOfWeek))
                return true;
        }
        return false;
    }

    private bool ExceedsMaxDailyHours(Teacher teacher, TimeSlot slot,
        List<Schedule> allSchedules, Dictionary<Guid, Schedule> grid, List<ScheduleConstraint> constraints)
    {
        var constraint = constraints.FirstOrDefault(c =>
            c.Type == ConstraintType.MaxDailyHours && c.Hardness == ConstraintHardness.Hard
            && (c.TeacherId == null || c.TeacherId == teacher.Id));

        if (constraint == null) return false;

        int maxHours = 6; // Default
        if (!string.IsNullOrEmpty(constraint.Parameters))
        {
            try { maxHours = System.Text.Json.JsonSerializer.Deserialize<MaxHoursParam>(constraint.Parameters)?.MaxHours ?? 6; }
            catch { }
        }

        int currentDayHours = allSchedules.Count(s => s.TeacherId == teacher.Id
            && s.TimeSlot?.DayOfWeek == slot.DayOfWeek)
            + grid.Values.Count(s => s.TeacherId == teacher.Id);

        return currentDayHours >= maxHours;
    }

    // ── EF slot rules (kept from original) ───────────────────────────────

    private bool IsEfSlotValid(GradeLevel grade, TimeSlot slot) =>
        IsEfSlotReservedForGrade(grade, slot);

    private bool IsEfSlotReservedForGrade(GradeLevel grade, TimeSlot slot)
    {
        var startLimit = new TimeSpan(10, 0, 0);
        var endLimit = new TimeSpan(13, 0, 0);
        if (slot.StartTime < startLimit || slot.EndTime > endLimit) return false;

        return grade switch
        {
            GradeLevel.Primary3 => slot.DayOfWeek == Domain.Entities.DayOfWeek.Tuesday,
            GradeLevel.Primary4 => slot.DayOfWeek == Domain.Entities.DayOfWeek.Wednesday,
            GradeLevel.Primary5 => slot.DayOfWeek == Domain.Entities.DayOfWeek.Thursday,
            GradeLevel.Primary6 => slot.DayOfWeek == Domain.Entities.DayOfWeek.Friday,
            _ => false
        };
    }

    // ── Scoring ──────────────────────────────────────────────────────────

    private double CalculateScore(List<Schedule> schedules, List<ScheduleConstraint> constraints) =>
        schedules.Count > 0 ? 75.0 : 0;

    private double CalculateTeacherSatisfaction(List<Schedule> schedules,
        List<TeacherAvailability> availabilities, List<string> details)
    {
        if (schedules.Count == 0) return 0;

        int preferred = 0, undesired = 0, total = schedules.Count;
        foreach (var s in schedules)
        {
            var avail = availabilities.FirstOrDefault(a => a.TeacherId == s.TeacherId && a.TimeSlotId == s.TimeSlotId);
            if (avail?.Level == AvailabilityLevel.Preferred) preferred++;
            if (avail?.Level == AvailabilityLevel.Undesired) undesired++;
        }

        double score = 100.0 * (total - undesired + preferred) / (total + preferred);
        details.Add($"Preferencias: {preferred} preferidas, {undesired} indeseables de {total} sesiones");
        return Math.Min(100, score);
    }

    private double CalculateCompactness(List<Schedule> schedules, List<string> details)
    {
        if (schedules.Count == 0) return 0;

        var byTeacherDay = schedules
            .Where(s => s.TimeSlot != null)
            .GroupBy(s => new { s.TeacherId, s.TimeSlot!.DayOfWeek });

        int totalGaps = 0, totalTeacherDays = 0;
        foreach (var group in byTeacherDay)
        {
            totalTeacherDays++;
            var times = group.Select(s => s.TimeSlot!.StartTime).OrderBy(t => t).ToList();
            for (int i = 1; i < times.Count; i++)
            {
                var gap = times[i] - times[i - 1];
                if (gap > TimeSpan.FromHours(1.5)) totalGaps++;
            }
        }

        double score = totalTeacherDays > 0 ? 100.0 * (1 - (double)totalGaps / totalTeacherDays) : 100;
        details.Add($"Compresión: {totalGaps} huecos en {totalTeacherDays} días-profesor");
        return Math.Max(0, score);
    }

    private double CalculateBalance(List<Schedule> schedules, List<string> details)
    {
        if (schedules.Count == 0) return 0;

        var byTeacher = schedules.GroupBy(s => s.TeacherId);
        var loads = byTeacher.Select(g => g.Count()).ToList();

        if (loads.Count <= 1) return 100;

        double avg = loads.Average();
        double variance = loads.Sum(l => Math.Pow(l - avg, 2)) / loads.Count;
        double stdDev = Math.Sqrt(variance);
        double score = Math.Max(0, 100 - stdDev * 10);

        details.Add($"Balance: desviación estándar de carga = {stdDev:F1}");
        return score;
    }

    // ── DTOs ─────────────────────────────────────────────────────────────

    private class PendingAssignment
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public bool IsEf { get; set; }
        public Guid? RequiredRoomId { get; set; }
    }

    private record MaxHoursParam(int MaxHours);
}
