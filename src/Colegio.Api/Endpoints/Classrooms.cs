using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class ClassroomsEndpoints
{
    public static void MapClassroomsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/classrooms", GetAllClassrooms);
        app.MapGet("/api/classrooms/{id}", GetClassroomById);
        app.MapPost("/api/classrooms", CreateClassroom);
        app.MapPut("/api/classrooms/{id}", UpdateClassroom);
        app.MapDelete("/api/classrooms/{id}", DeleteClassroom);
    }

    private static async Task<IResult> GetAllClassrooms(ColegioDbContext db)
    {
        var classrooms = await db.Classrooms
            .AsNoTracking()
            .Include(c => c.Tutor)
            .Include(c => c.Students)
            .ToListAsync();
        return Results.Ok(classrooms);
    }

    private static async Task<IResult> GetClassroomById(ColegioDbContext db, Guid id)
    {
        var classroom = await db.Classrooms
            .AsNoTracking()
            .Include(c => c.Tutor)
            .Include(c => c.Students)
            .FirstOrDefaultAsync(c => c.Id == id);
        return classroom is null ? Results.NotFound() : Results.Ok(classroom);
    }

    private static async Task<IResult> CreateClassroom(ColegioDbContext db, Classroom classroom)
    {
        classroom.Id = Guid.NewGuid();
        db.Classrooms.Add(classroom);
        await db.SaveChangesAsync();
        return Results.Created($"/api/classrooms/{classroom.Id}", classroom);
    }

    private static async Task<IResult> UpdateClassroom(ColegioDbContext db, Guid id, Classroom updated)
    {
        var classroom = await db.Classrooms.FirstOrDefaultAsync(c => c.Id == id);
        if (classroom is null) return Results.NotFound();

        classroom.GradeLevel = updated.GradeLevel;
        classroom.Line = updated.Line;
        classroom.SchoolId = updated.SchoolId;
        classroom.TutorId = updated.TutorId;

        db.Classrooms.Update(classroom);
        await db.SaveChangesAsync();
        return Results.Ok(classroom);
    }

    private static async Task<IResult> DeleteClassroom(ColegioDbContext db, Guid id)
    {
        var classroom = await db.Classrooms.FirstOrDefaultAsync(c => c.Id == id);
        if (classroom is null) return Results.NotFound();

        db.Classrooms.Remove(classroom);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}