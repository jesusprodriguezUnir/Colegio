namespace Colegio.Domain.Entities;

public class School
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();
}