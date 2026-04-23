namespace Colegio.Domain.Entities;

public enum AvailabilityLevel
{
    Available,     // Disponible sin preferencia
    Preferred,     // Horario preferido por el profesor
    Undesired,     // Prefiere no dar clase en esta franja
    Unavailable    // No disponible (restricción dura)
}

public class TeacherAvailability
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid TimeSlotId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public AvailabilityLevel Level { get; set; } = AvailabilityLevel.Available;

    public Teacher Teacher { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
}
