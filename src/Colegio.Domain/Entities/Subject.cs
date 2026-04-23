using System.ComponentModel.DataAnnotations;

namespace Colegio.Domain.Entities;

public class Subject
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Curriculum> Curriculums { get; set; } = new List<Curriculum>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
