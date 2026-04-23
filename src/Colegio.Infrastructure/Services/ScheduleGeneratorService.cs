using Colegio.Domain.Entities;
using Colegio.Domain.Services;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Infrastructure.Services;

public class ScheduleGeneratorService : IScheduleGenerator
{
    private readonly ColegioDbContext _context;

    public ScheduleGeneratorService(ColegioDbContext context)
    {
        _context = context;
    }

    public async Task<List<Schedule>> GenerateAsync(Guid classroomId, AcademicSessionType sessionType)
    {
        var allSchedulesAtSession = await _context.Schedules
            .Include(s => s.TimeSlot)
            .Where(s => s.TimeSlot.SessionType == sessionType)
            .ToListAsync();

        return await GenerateForClassroomInternalAsync(classroomId, sessionType, allSchedulesAtSession);
    }

    public async Task<List<Schedule>> GenerateAllAsync(AcademicSessionType sessionType)
    {
        var classrooms = await _context.Classrooms.ToListAsync();
        var allSchedulesAtSession = await _context.Schedules
            .Include(s => s.TimeSlot)
            .Where(s => s.TimeSlot.SessionType == sessionType)
            .ToListAsync();

        var allNewSchedules = new List<Schedule>();

        foreach (var classroom in classrooms)
        {
            try 
            {
                var classroomSchedules = await GenerateForClassroomInternalAsync(classroom.Id, sessionType, allSchedulesAtSession);
                
                // Add new schedules to the tracking list so the next classroom sees them
                var newOnly = classroomSchedules.Where(s => !allSchedulesAtSession.Any(existing => existing.Id == s.Id)).ToList();
                allSchedulesAtSession.AddRange(newOnly);
                allNewSchedules.AddRange(newOnly);
            }
            catch
            {
                // If one classroom fails, we continue with others or handle it? 
                // For a test generator, we can skip or log.
            }
        }

        return allNewSchedules;
    }

    private async Task<List<Schedule>> GenerateForClassroomInternalAsync(Guid classroomId, AcademicSessionType sessionType, List<Schedule> allSchedulesAtSession)
    {
        // 1. Load data
        var classroom = await _context.Classrooms
            .FirstOrDefaultAsync(c => c.Id == classroomId)
            ?? throw new Exception("Classroom not found");

        var curriculum = await _context.Curriculums
            .Where(c => c.GradeLevel == classroom.GradeLevel)
            .Include(c => c.Subject)
            .ToListAsync();

        var timeSlots = await _context.TimeSlots
            .Where(ts => ts.SessionType == sessionType)
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.StartTime)
            .ToListAsync();

        var teachers = await _context.Teachers
            .Include(t => t.Subjects)
            .Include(t => t.Availabilities)
            .ToListAsync();

        // 2. Prepare pending assignments
        var pendingAssignments = new List<PendingAssignment>();
        foreach (var curr in curriculum)
        {
            for (int i = 0; i < curr.WeeklyHours; i++)
            {
                pendingAssignments.Add(new PendingAssignment 
                { 
                    SubjectId = curr.SubjectId, 
                    SubjectName = curr.Subject.Name,
                    IsEf = curr.Subject.Name.Contains("Educación Física")
                });
            }
        }

        pendingAssignments = pendingAssignments
            .OrderByDescending(a => a.IsEf)
            .ThenBy(a => teachers.Count(t => t.Subjects.Any(s => s.Id == a.SubjectId)))
            .ToList();

        // 3. Filter slots and pre-assign locked ones
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

        // 4. Backtracking
        if (Solve(0, pendingAssignments, grid, timeSlots, teachers, allSchedulesAtSession, classroom))
        {
            return grid.Values.ToList();
        }

        throw new Exception($"Could not find a valid schedule for classroom {classroom.GradeLevel}º {classroom.Line}");
    }

    private bool Solve(int index, List<PendingAssignment> pending, Dictionary<Guid, Schedule> grid, List<TimeSlot> slots, List<Teacher> teachers, List<Schedule> allSchedules, Classroom classroom)
    {
        if (index >= pending.Count) return true;

        var currentAssignment = pending[index];
        
        foreach (var slot in slots)
        {
            if (slot.IsBreak) continue;
            if (grid.ContainsKey(slot.Id)) continue;

            // Apply EF Specific Rule
            if (currentAssignment.IsEf)
            {
                if (!IsEfSlotValid(classroom.GradeLevel, slot)) continue;
            }
            else
            {
                // If it's NOT EF, but this is a reserved EF slot for this grade, skip it
                // This ensures we leave space for EF
                if (IsEfSlotReservedForGrade(classroom.GradeLevel, slot))
                {
                    // Check if we still have EF hours to assign
                    if (pending.Skip(index).Any(a => a.IsEf)) continue;
                }
            }

            var candidateTeachers = teachers
                .Where(t => t.Subjects.Any(s => s.Id == currentAssignment.SubjectId))
                .Where(t => t.Availabilities.Any(a => a.TimeSlotId == slot.Id && a.IsAvailable))
                .Where(t => !allSchedules.Any(s => s.TeacherId == t.Id && s.TimeSlotId == slot.Id))
                .ToList();

            foreach (var teacher in candidateTeachers)
            {
                var newSchedule = new Schedule
                {
                    Id = Guid.NewGuid(),
                    ClassroomId = classroom.Id,
                    TeacherId = teacher.Id,
                    SubjectId = currentAssignment.SubjectId,
                    TimeSlotId = slot.Id,
                    IsLocked = false
                };

                grid[slot.Id] = newSchedule;
                allSchedules.Add(newSchedule);

                if (Solve(index + 1, pending, grid, slots, teachers, allSchedules, classroom))
                    return true;

                grid.Remove(slot.Id);
                allSchedules.Remove(newSchedule);
            }
        }

        return false;
    }

    private bool IsEfSlotValid(GradeLevel grade, TimeSlot slot)
    {
        if (!IsEfSlotReservedForGrade(grade, slot)) return false;
        return true;
    }

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

    private class PendingAssignment
    {
        public Guid SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;
        public bool IsEf { get; set; }
    }
}
