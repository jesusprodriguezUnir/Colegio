namespace Colegio.Domain.Entities;

public enum GradeLevel
{
    Infantile3 = 3,
    Infantile4 = 4,
    Infantile5 = 5,
    Primary1 = 1,
    Primary2 = 2,
    Primary3 = 3,
    Primary4 = 4,
    Primary5 = 5,
    Primary6 = 6
}

public enum ClassroomLine
{
    A,
    B
}

public class Classroom
{
    public Guid Id { get; set; }
    public GradeLevel GradeLevel { get; set; }
    public ClassroomLine Line { get; set; }
    public Guid SchoolId { get; set; }
    public Guid? TutorId { get; set; }

    public School School { get; set; } = null!;
    public Teacher? Tutor { get; set; }

    public ICollection<Student> Students { get; set; } = new List<Student>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}