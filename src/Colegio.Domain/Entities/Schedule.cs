using System.Text.Json.Serialization;

namespace Colegio.Domain.Entities;

public enum ScheduleType
{
    ClassUnit,       // Sesión docente normal
    OnCall,          // Guardia
    Meeting,         // Reunión
    Complementary    // Actividad complementaria
}

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
    public ScheduleType Type { get; set; } = ScheduleType.ClassUnit;
    public Guid? RoomId { get; set; }

    [JsonIgnore]
    public Classroom Classroom { get; set; } = null!;
    [JsonIgnore]
    public Teacher Teacher { get; set; } = null!;
    [JsonIgnore]
    public Subject Subject { get; set; } = null!;
    [JsonIgnore]
    public TimeSlot TimeSlot { get; set; } = null!;
    [JsonIgnore]
    public Room? Room { get; set; }
}