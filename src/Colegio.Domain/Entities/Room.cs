using System.Text.Json.Serialization;

namespace Colegio.Domain.Entities;

public enum RoomType
{
    Generic,    // Aula normal asignada a un grupo
    Specific,   // Aula especializada (Lab, Gimnasio, etc.)
    Shared      // Aula compartida (Salón de actos, etc.)
}

public class Room
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public RoomType Type { get; set; } = RoomType.Generic;
    public int Capacity { get; set; }
    public string? Building { get; set; }
    public int? Floor { get; set; }
    public string? Description { get; set; }

    [JsonIgnore]
    public ICollection<Schedule> Schedules { get; set; } = new List<Schedule>();
}
