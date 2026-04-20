namespace Colegio.Domain.Entities;

public enum GradeLevel
{
    Infantile3 = 1,
    Infantile4 = 2,
    Infantile5 = 3,
    Primary1 = 4,
    Primary2 = 5,
    Primary3 = 6,
    Primary4 = 7,
    Primary5 = 8,
    Primary6 = 9
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