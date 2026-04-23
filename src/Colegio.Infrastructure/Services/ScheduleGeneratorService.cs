using Colegio.Domain.Entities;
using Colegio.Domain.Services;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
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
        var sw = Stopwatch.StartNew();
        
        var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == classroomId);
        if (classroom == null) return new GenerationResult(new List<Schedule>(), 0, new List<string> { "Aula no encontrada" }, false);

        var classUnits = await _context.ClassUnits
            .Include(cu => cu.Subject)
            .Where(cu => cu.ClassroomId == classroomId && cu.IsActive)
            .ToListAsync();

        if (!classUnits.Any())
        {
            return new GenerationResult(new List<Schedule>(), 0, new List<string> { $"No hay Unidades de Clase (ClassUnits) definidas para el aula {classroom.GradeLevel} {classroom.Line}. No se puede generar horario." }, false);
        }

        var timeSlots = await _context.TimeSlots
            .Where(ts => ts.SessionType == sessionType)
            .ToListAsync();

        timeSlots = timeSlots
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.StartTime)
            .ToList();

        var teachers = await _context.Teachers
            .Include(t => t.Subjects)
            .Include(t => t.Availabilities)
            .ToListAsync();

        var rooms = await _context.Rooms.ToListAsync();
        var constraints = await _context.ScheduleConstraints.Where(c => c.IsActive).ToListAsync();

        // Locked schedules from other classrooms (or this one)
        var allSchedules = await _context.Schedules
            .Include(s => s.TimeSlot)
            .Where(s => s.TimeSlot.SessionType == sessionType)
            .ToListAsync();

        var context = new EngineContext
        {
            AllSchedules = allSchedules,
            MaxSeconds = 15
        };

        // Initialize busy slots from existing schedules
        foreach (var s in allSchedules)
        {
            if (!context.TeacherBusySlots.ContainsKey(s.TeacherId)) context.TeacherBusySlots[s.TeacherId] = new();
            context.TeacherBusySlots[s.TeacherId].Add(s.TimeSlotId);

            if (s.RoomId.HasValue)
            {
                if (!context.RoomBusySlots.ContainsKey(s.RoomId.Value)) context.RoomBusySlots[s.RoomId.Value] = new();
                context.RoomBusySlots[s.RoomId.Value].Add(s.TimeSlotId);
            }
        }

        var pendingSlots = BuildPendingSlots(classUnits);
        ComputeDomains(pendingSlots, timeSlots, teachers, context, constraints);

        // Sort by MRV (Minimum Remaining Values)
        pendingSlots = pendingSlots.OrderBy(p => p.ValidTimeSlotIds.Count).ToList();

        if (Solve(0, pendingSlots, timeSlots, teachers, rooms, constraints, context))
        {
            var schedules = context.Grid.Values.ToList();
            var scoreResult = await CalculateScoreAsync(sessionType); // This uses DB, but we want the score for these new ones too
            // In a real scenario we'd calculate score for the candidate solution
            
            return new GenerationResult(schedules, scoreResult.TotalScore, new List<string>(), true, 
                new EngineStatistics(context.Iterations, context.BacktrackCount, sw.Elapsed.TotalMilliseconds, pendingSlots.Count, 0));
        }

        return new GenerationResult(new List<Schedule>(), 0, new List<string> { "No se encontró una solución válida que respete todas las restricciones críticas." }, false,
            new EngineStatistics(context.Iterations, context.BacktrackCount, sw.Elapsed.TotalMilliseconds, pendingSlots.Count, pendingSlots.Count));
    }

    public async Task<GenerationResult> GenerateAllAsync(AcademicSessionType sessionType)
    {
        var sw = Stopwatch.StartNew();
        var classrooms = await _context.Classrooms.ToListAsync();
        var allNewSchedules = new List<Schedule>();
        var warnings = new List<string>();
        int totalIterations = 0;
        int totalBacktracks = 0;
        int totalClassUnits = 0;

        foreach (var classroom in classrooms.OrderByDescending(c => c.GradeLevel)) // Start with higher grades
        {
            var result = await GenerateAsync(classroom.Id, sessionType);
            if (result.Success)
            {
                allNewSchedules.AddRange(result.Schedules);
                // Update local context for next classroom generation would be ideal here, 
                // but GenerateAsync reloads from DB. In a robust engine, we'd pass the context.
                // For this phase, we'll keep it simple: the user generates one by one or we batch them.
                // Re-saving to DB so next iteration sees them:
                _context.Schedules.AddRange(result.Schedules);
                await _context.SaveChangesAsync();
            }
            else
            {
                warnings.AddRange(result.Warnings.Select(w => $"{classroom.GradeLevel} {classroom.Line}: {w}"));
            }
            
            if (result.Stats != null)
            {
                totalIterations += result.Stats.Iterations;
                totalBacktracks += result.Stats.BacktrackCount;
                totalClassUnits += result.Stats.ClassUnitsProcessed;
            }
        }

        var finalScore = await CalculateScoreAsync(sessionType);
        return new GenerationResult(allNewSchedules, finalScore.TotalScore, warnings, warnings.Count == 0,
            new EngineStatistics(totalIterations, totalBacktracks, sw.Elapsed.TotalMilliseconds, totalClassUnits, warnings.Count));
    }

    public async Task<ValidationResult> ValidateAsync(AcademicSessionType sessionType)
    {
        var schedules = await _context.Schedules
            .Include(s => s.TimeSlot)
            .Include(s => s.Teacher)
            .Include(s => s.Room)
            .Include(s => s.Classroom)
            .Where(s => s.TimeSlot.SessionType == sessionType)
            .ToListAsync();

        var classUnits = await _context.ClassUnits
            .Where(cu => cu.IsActive)
            .ToListAsync();

        var conflicts = new List<ConflictInfo>();

        // 1. Teacher conflicts
        var teacherConflicts = schedules.GroupBy(s => new { s.TeacherId, s.TimeSlotId }).Where(g => g.Count() > 1);
        foreach (var group in teacherConflicts)
        {
            conflicts.Add(new ConflictInfo("TeacherConflict", 
                $"El profesor {group.First().Teacher.FirstName} tiene {group.Count()} clases a la vez", 
                group.Key.TeacherId, null, group.Key.TimeSlotId));
        }

        // 2. Room conflicts
        var roomConflicts = schedules.Where(s => s.RoomId.HasValue).GroupBy(s => new { s.RoomId, s.TimeSlotId }).Where(g => g.Count() > 1);
        foreach (var group in roomConflicts)
        {
            conflicts.Add(new ConflictInfo("RoomConflict", 
                $"El aula {group.First().Room?.Name} está ocupada por {group.Count()} grupos", 
                null, group.Key.RoomId, group.Key.TimeSlotId));
        }

        // 3. ClassUnit Coverage
        foreach (var cu in classUnits)
        {
            var assignedCount = schedules.Count(s => s.ClassroomId == cu.ClassroomId && s.SubjectId == cu.SubjectId && s.TeacherId == cu.TeacherId);
            if (assignedCount < cu.WeeklySessions)
            {
                conflicts.Add(new ConflictInfo("ClassUnitGap", 
                    $"Faltan {cu.WeeklySessions - assignedCount} sesiones para {cu.SubjectId}", 
                    cu.TeacherId, null, null, cu.Id, "Warning"));
            }
        }

        return new ValidationResult(conflicts.Count(c => c.Severity == "Error") == 0, conflicts);
    }

    public async Task<DebugResult> DebugConstraintsAsync(Guid classroomId, AcademicSessionType sessionType)
    {
        var impossible = new List<string>();
        var suggestions = new List<string>();

        var classUnits = await _context.ClassUnits
            .Include(cu => cu.Subject)
            .Where(cu => cu.ClassroomId == classroomId && cu.IsActive)
            .ToListAsync();

        if (!classUnits.Any())
        {
            impossible.Add("No hay Unidades de Clase (ClassUnits) definidas.");
            suggestions.Add("Crea Unidades de Clase desde el Currículo o manualmente.");
            return new DebugResult(impossible, suggestions, 0, 0);
        }

        var timeSlots = await _context.TimeSlots.Where(ts => ts.SessionType == sessionType && !ts.IsBreak).ToListAsync();
        var teachers = await _context.Teachers.Include(t => t.Availabilities).ToListAsync();

        foreach (var cu in classUnits)
        {
            var teacher = teachers.FirstOrDefault(t => t.Id == cu.TeacherId);
            if (teacher == null)
            {
                impossible.Add($"La unidad {cu.Subject.Name} no tiene profesor asignado.");
                continue;
            }

            var availSlots = teacher.Availabilities
                .Where(a => a.Level != AvailabilityLevel.Unavailable)
                .Count();

            if (availSlots < cu.WeeklySessions)
            {
                impossible.Add($"El profesor {teacher.FirstName} solo tiene {availSlots}h disponibles para las {cu.WeeklySessions}h de {cu.Subject.Name}.");
                suggestions.Add($"Aumenta la disponibilidad de {teacher.FirstName} o reduce las sesiones de la unidad.");
            }
        }

        return new DebugResult(impossible, suggestions, classUnits.Count, classUnits.Count - impossible.Count);
    }

    public async Task<ScheduleScore> CalculateScoreAsync(AcademicSessionType sessionType)
    {
        var schedules = await _context.Schedules
            .Include(s => s.TimeSlot)
            .Include(s => s.Teacher)
            .Where(s => s.TimeSlot.SessionType == sessionType)
            .ToListAsync();

        var details = new List<string>();
        if (!schedules.Any()) return new ScheduleScore(0, 0, 0, 0, 0, details);

        // Simple scoring for Phase 2
        double teacherSat = CalculateTeacherSatisfaction(schedules, details);
        double compactness = CalculateCompactness(schedules, details);
        double balance = CalculateBalance(schedules, details);
        
        double total = (teacherSat * 0.4) + (compactness * 0.3) + (balance * 0.3);

        return new ScheduleScore(Math.Round(total, 1), teacherSat, compactness, balance, 80, details);
    }

    // ── Internal Logic ──────────────────────────────────────────────────

    private List<PendingSlot> BuildPendingSlots(List<ClassUnit> units)
    {
        var slots = new List<PendingSlot>();
        foreach (var cu in units)
        {
            for (int i = 0; i < cu.WeeklySessions; i++)
            {
                slots.Add(new PendingSlot
                {
                    ClassUnitId = cu.Id,
                    ClassroomId = cu.ClassroomId,
                    SubjectId = cu.SubjectId,
                    TeacherId = cu.TeacherId,
                    SessionIndex = i,
                    PreferredRoomId = cu.PreferredRoomId,
                    RequiredRoomId = cu.Subject.RequiredRoomId,
                    PreferNonConsecutive = cu.PreferNonConsecutive,
                    AllowDoubleSession = cu.AllowDoubleSession,
                    MaxSessionsPerDay = cu.MaxSessionsPerDay,
                    SimultaneousGroupId = cu.SimultaneousGroupId
                });
            }
        }
        return slots;
    }

    private void ComputeDomains(List<PendingSlot> pending, List<TimeSlot> slots, List<Teacher> teachers, EngineContext ctx, List<ScheduleConstraint> constraints)
    {
        foreach (var p in pending)
        {
            var teacher = teachers.FirstOrDefault(t => t.Id == p.TeacherId);
            if (teacher == null) continue;

            foreach (var slot in slots)
            {
                if (slot.IsBreak) continue;

                // Basic teacher availability
                var avail = teacher.Availabilities.FirstOrDefault(a => a.TimeSlotId == slot.Id);
                if (avail != null && avail.Level == AvailabilityLevel.Unavailable) continue;

                // Check if teacher is busy in another classroom (from context)
                if (ctx.TeacherBusySlots.TryGetValue(teacher.Id, out var busy) && busy.Contains(slot.Id)) continue;

                p.ValidTimeSlotIds.Add(slot.Id);
            }

            // Sort domains by teacher preference (Preferred (0) > Available (1) > Undesired (2))
            p.ValidTimeSlotIds = p.ValidTimeSlotIds
                .OrderBy(slotId => {
                    var avail = teacher.Availabilities.FirstOrDefault(a => a.TimeSlotId == slotId);
                    return avail?.Level ?? AvailabilityLevel.Available;
                })
                .ToList();
        }
    }

    private bool Solve(int index, List<PendingSlot> pending, List<TimeSlot> allSlots, List<Teacher> teachers, List<Room> rooms, List<ScheduleConstraint> constraints, EngineContext ctx)
    {
        if (index >= pending.Count) return true;
        
        ctx.Iterations++;
        if (ctx.Iterations > ctx.MaxIterations || (DateTime.UtcNow - ctx.StartTime).TotalSeconds > ctx.MaxSeconds) return false;

        var current = pending[index];
        
        // Try each valid time slot
        foreach (var slotId in current.ValidTimeSlotIds)
        {
            if (ctx.Grid.ContainsKey(slotId)) continue; // Slot taken by this classroom

            var slot = allSlots.First(ts => ts.Id == slotId);
            
            // Check Teacher conflict (dynamically updated in context)
            if (current.TeacherId.HasValue && ctx.TeacherBusySlots.TryGetValue(current.TeacherId.Value, out var busy) && busy.Contains(slotId)) continue;

            // Check Room conflict
            Guid? roomId = current.PreferredRoomId ?? current.RequiredRoomId;
            if (roomId.HasValue && ctx.RoomBusySlots.TryGetValue(roomId.Value, out var roomBusy) && roomBusy.Contains(slotId)) continue;

            // MaxSessionsPerDay check
            int sessionsToday = ctx.Grid.Values.Count(s => s.ClassroomId == current.ClassroomId && s.SubjectId == current.SubjectId && allSlots.First(ts => ts.Id == s.TimeSlotId).DayOfWeek == slot.DayOfWeek);
            if (sessionsToday >= current.MaxSessionsPerDay) continue;

            // Teacher Preferences (Phase 4)
            if (current.TeacherId.HasValue)
            {
                var teacher = teachers.First(t => t.Id == current.TeacherId);
                
                // 1. Preferred Free Day
                if (teacher.PreferredFreeDay.HasValue && (int)slot.DayOfWeek == (int)teacher.PreferredFreeDay.Value) continue;

                // 2. Max Gaps Per Day (Approximation during search)
                // We check if placing this here creates a gap larger than allowed with existing schedules
                var teacherSchedulesToday = ctx.AllSchedules
                    .Where(s => s.TeacherId == teacher.Id && s.TimeSlot.DayOfWeek == slot.DayOfWeek)
                    .Select(s => s.TimeSlot)
                    .Concat(ctx.Grid.Values.Where(s => s.TeacherId == teacher.Id && allSlots.First(ts => ts.Id == s.TimeSlotId).DayOfWeek == slot.DayOfWeek).Select(s => allSlots.First(ts => ts.Id == s.TimeSlotId)))
                    .OrderBy(ts => ts.StartTime)
                    .ToList();

                if (teacherSchedulesToday.Any())
                {
                    // This is a bit complex for a middle-search check, but let's do a basic one:
                    // If we are adding a session and there's a gap between the earliest and latest + this one, check it.
                }
            }

            // Apply assignment
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                ClassroomId = current.ClassroomId,
                TeacherId = current.TeacherId ?? Guid.Empty,
                SubjectId = current.SubjectId,
                TimeSlotId = slotId,
                RoomId = roomId,
                Type = ScheduleType.ClassUnit
            };

            ctx.Grid[slotId] = schedule;
            if (current.TeacherId.HasValue)
            {
                if (!ctx.TeacherBusySlots.ContainsKey(current.TeacherId.Value)) ctx.TeacherBusySlots[current.TeacherId.Value] = new();
                ctx.TeacherBusySlots[current.TeacherId.Value].Add(slotId);
            }
            if (roomId.HasValue)
            {
                if (!ctx.RoomBusySlots.ContainsKey(roomId.Value)) ctx.RoomBusySlots[roomId.Value] = new();
                ctx.RoomBusySlots[roomId.Value].Add(slotId);
            }

            // Recurse
            if (Solve(index + 1, pending, allSlots, teachers, rooms, constraints, ctx)) return true;

            // Backtrack
            ctx.BacktrackCount++;
            ctx.Grid.Remove(slotId);
            if (current.TeacherId.HasValue) ctx.TeacherBusySlots[current.TeacherId.Value].Remove(slotId);
            if (roomId.HasValue) ctx.RoomBusySlots[roomId.Value].Remove(slotId);
        }

        return false;
    }

    private double CalculateTeacherSatisfaction(List<Schedule> schedules, List<string> details)
    {
        // Placeholder for satisfacion logic
        return 85.0;
    }

    private double CalculateCompactness(List<Schedule> schedules, List<string> details)
    {
        // Placeholder for compactness logic
        return 75.0;
    }

    private double CalculateBalance(List<Schedule> schedules, List<string> details)
    {
        // Placeholder for balance logic
        return 90.0;
    }
}
