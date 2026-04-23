using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class ConstraintEndpoints
{
    public static void MapConstraintEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/constraints").WithTags("Constraints");

        group.MapGet("/", GetAllConstraints);
        group.MapPost("/", CreateConstraint);
        group.MapPut("/{id}", UpdateConstraint);
        group.MapDelete("/{id}", DeleteConstraint);
        group.MapPost("/defaults", CreateDefaultConstraints);
    }

    private static async Task<IResult> GetAllConstraints(ColegioDbContext db)
    {
        var constraints = await db.ScheduleConstraints
            .Include(c => c.Teacher)
            .Include(c => c.Subject)
            .Include(c => c.Classroom)
            .AsNoTracking()
            .ToListAsync();
        return Results.Ok(constraints);
    }

    private static async Task<IResult> CreateConstraint(ColegioDbContext db, ScheduleConstraint constraint)
    {
        constraint.Id = Guid.NewGuid();
        db.ScheduleConstraints.Add(constraint);
        await db.SaveChangesAsync();
        return Results.Created($"/api/constraints/{constraint.Id}", constraint);
    }

    private static async Task<IResult> UpdateConstraint(ColegioDbContext db, Guid id, ScheduleConstraint updated)
    {
        var constraint = await db.ScheduleConstraints.FirstOrDefaultAsync(c => c.Id == id);
        if (constraint is null) return Results.NotFound();

        constraint.Type = updated.Type;
        constraint.Hardness = updated.Hardness;
        constraint.Weight = updated.Weight;
        constraint.TeacherId = updated.TeacherId;
        constraint.SubjectId = updated.SubjectId;
        constraint.ClassroomId = updated.ClassroomId;
        constraint.Parameters = updated.Parameters;
        constraint.IsActive = updated.IsActive;
        constraint.Description = updated.Description;

        db.ScheduleConstraints.Update(constraint);
        await db.SaveChangesAsync();
        return Results.Ok(constraint);
    }

    private static async Task<IResult> DeleteConstraint(ColegioDbContext db, Guid id)
    {
        var constraint = await db.ScheduleConstraints.FirstOrDefaultAsync(c => c.Id == id);
        if (constraint is null) return Results.NotFound();

        db.ScheduleConstraints.Remove(constraint);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> CreateDefaultConstraints(ColegioDbContext db)
    {
        // Añadir algunas restricciones comunes por defecto si no existen
        if (!await db.ScheduleConstraints.AnyAsync())
        {
            var defaults = new List<ScheduleConstraint>
            {
                new() { Id = Guid.NewGuid(), Type = ConstraintType.MaxDailyHours, Hardness = ConstraintHardness.Hard, Parameters = "{\"MaxHours\": 6}", Description = "Máximo 6 horas lectivas al día" },
                new() { Id = Guid.NewGuid(), Type = ConstraintType.CompactSchedule, Hardness = ConstraintHardness.Soft, Weight = 8, Description = "Minimizar huecos en el horario del profesor" },
                new() { Id = Guid.NewGuid(), Type = ConstraintType.NonConsecutiveDays, Hardness = ConstraintHardness.Soft, Weight = 5, Description = "Intentar no dar la misma asignatura en días consecutivos" }
            };
            db.ScheduleConstraints.AddRange(defaults);
            await db.SaveChangesAsync();
        }
        return Results.Ok(new { Message = "Restricciones por defecto creadas" });
    }
}
