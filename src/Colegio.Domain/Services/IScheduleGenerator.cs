using Colegio.Domain.Entities;

namespace Colegio.Domain.Services;

public interface IScheduleGenerator
{
    Task<List<Schedule>> GenerateAsync(Guid classroomId, AcademicSessionType sessionType);
    Task<List<Schedule>> GenerateAllAsync(AcademicSessionType sessionType);
}
