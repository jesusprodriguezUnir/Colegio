namespace Colegio.Domain.Entities;

public class Student
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Guid ClassroomId { get; set; }

    public Classroom Classroom { get; set; } = null!;

    public ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}