using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class TeachersEndpoints
{
    public static void MapTeachersEndpoints(this WebApplication app)
    {
        app.MapGet("/api/teachers", GetAllTeachers);
        app.MapGet("/api/teachers/{id}", GetTeacherById);
        app.MapPost("/api/teachers", CreateTeacher);
        app.MapPut("/api/teachers/{id}", UpdateTeacher);
        app.MapDelete("/api/teachers/{id}", DeleteTeacher);
    }

    private static async Task<IResult> GetAllTeachers(ColegioDbContext db)
    {
        var teachers = await db.Teachers.AsNoTracking().ToListAsync();
        return Results.Ok(teachers);
    }

    private static async Task<IResult> GetTeacherById(ColegioDbContext db, Guid id)
    {
        var teacher = await db.Teachers.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
        return teacher is null ? Results.NotFound() : Results.Ok(teacher);
    }

    private static async Task<IResult> CreateTeacher(ColegioDbContext db, Teacher teacher)
    {
        teacher.Id = Guid.NewGuid();
        db.Teachers.Add(teacher);
        await db.SaveChangesAsync();
        return Results.Created($"/api/teachers/{teacher.Id}", teacher);
    }

    private static async Task<IResult> UpdateTeacher(ColegioDbContext db, Guid id, Teacher updated)
    {
        var teacher = await db.Teachers.FirstOrDefaultAsync(t => t.Id == id);
        if (teacher is null) return Results.NotFound();

        teacher.FirstName = updated.FirstName;
        teacher.LastName = updated.LastName;
        teacher.Specialty = updated.Specialty;
        teacher.HireDate = updated.HireDate;

        db.Teachers.Update(teacher);
        await db.SaveChangesAsync();
        return Results.Ok(teacher);
    }

    private static async Task<IResult> DeleteTeacher(ColegioDbContext db, Guid id)
    {
        var teacher = await db.Teachers.FirstOrDefaultAsync(t => t.Id == id);
        if (teacher is null) return Results.NotFound();

        db.Teachers.Remove(teacher);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}