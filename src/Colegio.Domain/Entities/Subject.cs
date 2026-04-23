using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Colegio.Domain.Entities;

public class Subject
{
    public Guid Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    public string Color { get; set; } = "#6366f1";
    
    public Guid? RequiredRoomId { get; set; }

    [JsonIgnore]
    public Room? RequiredRoom { get; set; }

    public ICollection<Curriculum> Curriculums { get; set; } = new List<Curriculum>();
    public ICollection<Teacher> Teachers { get; set; } = new List<Teacher>();
}
