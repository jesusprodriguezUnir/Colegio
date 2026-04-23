namespace Colegio.Domain.Entities;

public class TeacherAvailability
{
    public Guid Id { get; set; }
    public Guid TeacherId { get; set; }
    public Guid TimeSlotId { get; set; }
    public bool IsAvailable { get; set; } = true;

    public Teacher Teacher { get; set; } = null!;
    public TimeSlot TimeSlot { get; set; } = null!;
}
