using System.Text.Json.Serialization;

namespace Colegio.Domain.Entities;

public class Parent
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<StudentParent> StudentParents { get; set; } = new List<StudentParent>();
    [JsonIgnore]
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}