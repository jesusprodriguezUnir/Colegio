using Colegio.Domain.Entities;

namespace Colegio.Infrastructure.Services;

/// <summary>
/// A single session slot that needs to be placed on the timetable.
/// One ClassUnit with WeeklySessions=4 produces 4 PendingSlots.
/// </summary>
internal sealed class PendingSlot
{
    public Guid ClassUnitId { get; init; }
    public Guid ClassroomId { get; init; }
    public Guid SubjectId { get; init; }
    public Guid? TeacherId { get; init; }     // Pre-assigned or null
    public int SessionIndex { get; init; }     // 0..WeeklySessions-1
    public Guid? PreferredRoomId { get; init; }
    public Guid? RequiredRoomId { get; init; } // From Subject
    public bool PreferNonConsecutive { get; init; }
    public bool AllowDoubleSession { get; init; }
    public int MaxSessionsPerDay { get; init; }
    public Guid? SimultaneousGroupId { get; init; }
    
    // Computed domain (available time slots for this specific slot)
    public List<Guid> ValidTimeSlotIds { get; set; } = new();
}

internal sealed class EngineContext
{
    public Dictionary<Guid, Schedule> Grid { get; } = new();  // TimeSlotId → Schedule
    public List<Schedule> AllSchedules { get; init; } = new(); // Global schedules for conflict checking
    public Dictionary<Guid, HashSet<Guid>> TeacherBusySlots { get; } = new(); // TeacherId → busy TimeSlotIds
    public Dictionary<Guid, HashSet<Guid>> RoomBusySlots { get; } = new();    // RoomId → busy TimeSlotIds
    public Dictionary<Guid, HashSet<Guid>> ClassroomBusySlots { get; } = new(); // ClassroomId → busy TimeSlotIds
    public Dictionary<Guid, TimeSlot> SlotDetails { get; init; } = new();     // TimeSlotId → TimeSlot
    
    public int Iterations { get; set; }
    public int BacktrackCount { get; set; }
    public DateTime StartTime { get; init; } = DateTime.UtcNow;
    public int MaxIterations { get; init; } = 100_000;
    public int MaxSeconds { get; init; } = 30;
}
