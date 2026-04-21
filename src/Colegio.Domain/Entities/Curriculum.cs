namespace Colegio.Domain.Entities;

public class Curriculum
{
    public Guid Id { get; set; }
    public GradeLevel GradeLevel { get; set; }
    public Guid SubjectId { get; set; }
    public int WeeklyHours { get; set; }

    public Subject Subject { get; set; } = null!;
}
