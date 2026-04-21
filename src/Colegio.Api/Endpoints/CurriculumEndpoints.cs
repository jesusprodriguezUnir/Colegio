using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class CurriculumEndpoints
{
    public static void MapCurriculumEndpoints(this WebApplication app)
    {
        app.MapGet("/api/curriculum", GetAllCurriculums);
        app.MapGet("/api/curriculum/{gradeLevel}", GetCurriculumByGrade);
        app.MapGet("/api/subjects", GetAllSubjects);
    }

    private static async Task<IResult> GetAllCurriculums(ColegioDbContext db)
    {
        var curriculums = await db.Curriculums
            .AsNoTracking()
            .Include(c => c.Subject)
            .GroupBy(c => c.GradeLevel)
            .Select(g => new
            {
                GradeLevel = g.Key.ToString(),
                Subjects = g.Select(c => new
                {
                    c.Subject.Name,
                    c.WeeklyHours
                }).ToList()
            })
            .ToListAsync();
        
        return Results.Ok(curriculums);
    }

    private static async Task<IResult> GetCurriculumByGrade(ColegioDbContext db, GradeLevel gradeLevel)
    {
        var curriculum = await db.Curriculums
            .AsNoTracking()
            .Where(c => c.GradeLevel == gradeLevel)
            .Include(c => c.Subject)
            .Select(c => new
            {
                c.Subject.Name,
                c.WeeklyHours
            })
            .ToListAsync();
        
        return Results.Ok(curriculum);
    }

    private static async Task<IResult> GetAllSubjects(ColegioDbContext db)
    {
        var subjects = await db.Subjects
            .AsNoTracking()
            .OrderBy(s => s.Name)
            .ToListAsync();
        
        return Results.Ok(subjects);
    }
}
