using System.Text.Json.Serialization;

namespace Colegio.Domain.Entities;

/// <summary>
/// Represents a teaching assignment: a specific Subject taught to a specific Classroom
/// by a specific Teacher, with a defined number of weekly sessions.
/// This is the core planning unit that the schedule engine works with.
/// Inspired by Peñalara GHC "Class Units" (Unidades de Clase).
/// </summary>
public class ClassUnit
{
    public Guid Id { get; set; }
    
    // Core assignment
    public Guid ClassroomId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid? TeacherId { get; set; }        // Null = to be assigned by engine or manually
    
    // Session configuration
    public int WeeklySessions { get; set; }      // Number of sessions per week (from curriculum)
    public int SessionDuration { get; set; } = 1; // 1 = single period, 2 = double period
    
    // Scheduling preferences (inspired by GHC video 10)
    public bool AllowConsecutiveDays { get; set; } = true;   // Allow sessions on consecutive days
    public bool PreferNonConsecutive { get; set; } = true;    // Prefer spreading across the week
    public bool AllowDoubleSession { get; set; }              // Allow 2-hour blocks
    public int MaxSessionsPerDay { get; set; } = 1;           // Max sessions of this unit per day
    
    // Room preferences
    public Guid? PreferredRoomId { get; set; }   // Specific room preference (overrides subject default)
    
    // Grouping
    public Guid? SimultaneousGroupId { get; set; } // Units that must be scheduled at the same time (e.g., split groups)
    
    // Status
    public bool IsActive { get; set; } = true;

    // Navigation properties
    [JsonIgnore]
    public Classroom Classroom { get; set; } = null!;
    [JsonIgnore]
    public Subject Subject { get; set; } = null!;
    [JsonIgnore]
    public Teacher? Teacher { get; set; }
    [JsonIgnore]
    public Room? PreferredRoom { get; set; }
}
