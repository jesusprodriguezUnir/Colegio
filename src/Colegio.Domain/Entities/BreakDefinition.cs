using System;

namespace Colegio.Domain.Entities;

public class BreakDefinition
{
    public Guid Id { get; set; }
    public Guid TimetableFrameworkId { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Label { get; set; } = "Recreo";
    
    // Propiedad de navegación
    public TimetableFramework? Framework { get; set; }
}
