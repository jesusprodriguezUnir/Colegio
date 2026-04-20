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
    public string Subject { get; set; } = string.Empty;
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    [JsonIgnore]
    public Classroom Classroom { get; set; } = null!;
    [JsonIgnore]
    public Teacher Teacher { get; set; } = null!;
}