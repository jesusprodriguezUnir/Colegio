using System.Text.Json.Serialization;

namespace Colegio.Domain.Entities;

public class Teacher
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string IBAN { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public int MaxWorkingHours { get; set; }

    [JsonIgnore]
    public ICollection<Classroom> TutoredClassrooms { get; set; } = new List<Classroom>();
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}