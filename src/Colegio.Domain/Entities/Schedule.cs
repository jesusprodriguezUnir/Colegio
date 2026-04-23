using System.Text.Json.Serialization;

namespace Colegio.Domain.Entities;

public enum DayOfWeek
{
    Monday,
    Tuesday,
    Wednesday,
    Thursday,
    Friday
}

public class Schedule
{
    public Guid Id { get; set; }
    public Guid ClassroomId { get; set; }
    public Guid TeacherId { get; set; }
    public Guid SubjectId { get; set; }
    public Guid TimeSlotId { get; set; }
    public bool IsLocked { get; set; }

    [JsonIgnore]
    public Classroom Classroom { get; set; } = null!;
    [JsonIgnore]
    public Teacher Teacher { get; set; } = null!;
    [JsonIgnore]
    public Subject Subject { get; set; } = null!;
    [JsonIgnore]
    public TimeSlot TimeSlot { get; set; } = null!;
}