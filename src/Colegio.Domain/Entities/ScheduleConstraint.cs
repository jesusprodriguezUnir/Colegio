namespace Colegio.Domain.Entities;

public enum ConstraintType
{
    MaxDailyHours,           // Máx horas diarias para profesor
    MinDailyHours,           // Mín horas diarias para profesor
    MaxConsecutiveHours,     // Máx horas consecutivas sin descanso
    ConsecutiveSessions,     // Sesiones misma asignatura en días consecutivos
    NonConsecutiveDays,      // No dar misma asignatura en días consecutivos
    CompactSchedule,         // Compresión horaria del profesor (minimizar huecos)
    FreePeriodsBalance,      // Distribución equilibrada de horas libres
    PreferredTimeSlot,       // Preferencia de franja horaria
    AvoidTimeSlot,           // Evitar franja horaria
    RequireSpecificRoom,     // Requiere aula específica
    SubjectSpread,           // Distribuir asignatura en la semana
    DailyLoadBalance,        // Balance de carga diaria del profesor
    SameSubjectSameTime      // Misma asignatura a la misma hora en grupos paralelos
}

public enum ConstraintHardness
{
    Hard,   // Debe cumplirse obligatoriamente
    Soft    // Se intenta satisfacer, penalización si no
}

public class ScheduleConstraint
{
    public Guid Id { get; set; }
    public ConstraintType Type { get; set; }
    public ConstraintHardness Hardness { get; set; } = ConstraintHardness.Soft;
    public int Weight { get; set; } = 5;  // Peso 1-10 para restricciones blandas
    
    // Scope: a quién aplica (null = aplica globalmente)
    public Guid? TeacherId { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid? ClassroomId { get; set; }
    
    // Parámetros adicionales en JSON (e.g. {"maxHours": 6, "dayOfWeek": 1})
    public string? Parameters { get; set; }
    
    public bool IsActive { get; set; } = true;
    public string? Description { get; set; }

    public Teacher? Teacher { get; set; }
    public Subject? Subject { get; set; }
    public Classroom? Classroom { get; set; }
}
