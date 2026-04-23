namespace Colegio.Domain.Entities;

public enum AcademicSessionType
{
    Standard,   // Octubre a Mayo
    Intensive   // Junio y Septiembre
}

public class TimeSlot
{
    public Guid Id { get; set; }
    public AcademicSessionType SessionType { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsBreak { get; set; }
    public string Label { get; set; } = string.Empty;

    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
    public ICollection<TeacherAvailability> Availabilities { get; set; } = new List<TeacherAvailability>();
}
