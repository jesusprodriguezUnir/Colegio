using Colegio.Domain.Entities;

namespace Colegio.Domain.Services;

public interface IScheduleGenerator
{
    Task<GenerationResult> GenerateAsync(Guid classroomId, AcademicSessionType sessionType);
    Task<GenerationResult> GenerateAllAsync(AcademicSessionType sessionType);
    Task<ValidationResult> ValidateAsync(AcademicSessionType sessionType);
    Task<DebugResult> DebugConstraintsAsync(Guid classroomId, AcademicSessionType sessionType);
    Task<ScheduleScore> CalculateScoreAsync(AcademicSessionType sessionType);
}

public record GenerationResult(
    List<Schedule> Schedules, 
    double Score, 
    List<string> Warnings,
    bool Success,
    EngineStatistics? Stats = null);

public record EngineStatistics(
    int Iterations,
    int BacktrackCount, 
    double ElapsedMs,
    int ClassUnitsProcessed,
    int UnresolvedUnits);

public record ValidationResult(
    bool IsValid, 
    List<ConflictInfo> Conflicts);

public record ConflictInfo(
    string Type,           // "TeacherConflict", "RoomConflict", "CurriculumGap", "ClassUnitGap", etc.
    string Description,
    Guid? TeacherId,
    Guid? RoomId,
    Guid? TimeSlotId,
    Guid? ClassUnitId = null,
    string Severity = "Error");

public record DebugResult(
    List<string> ImpossibleConstraints, 
    List<string> Suggestions,
    int TotalConstraints,
    int SatisfiedConstraints);

public record ScheduleScore(
    double TotalScore,           // 0-100
    double TeacherSatisfaction,  // Preferencias de profesores
    double CompactnessScore,     // Compresión horaria
    double BalanceScore,         // Balance de carga
    double RoomOptimization,     // Uso óptimo de aulas
    List<string> Details);
