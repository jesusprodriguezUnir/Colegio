using System;
using System.Collections.Generic;

namespace Colegio.Domain.Entities;

public class TimetableFramework
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AcademicSessionType SessionType { get; set; }
    
    // Configuración de la jornada
    public bool HasAfternoon { get; set; }
    public TimeSpan MorningStart { get; set; }
    public TimeSpan MorningEnd { get; set; }
    public TimeSpan? AfternoonStart { get; set; }
    public TimeSpan? AfternoonEnd { get; set; }
    
    // Configuración de las sesiones
    public int SessionDurationMinutes { get; set; } = 60;
    
    // Relaciones
    public ICollection<BreakDefinition> Breaks { get; set; } = new List<BreakDefinition>();
    public ICollection<TimeSlot> GeneratedTimeSlots { get; set; } = new List<TimeSlot>();
}
