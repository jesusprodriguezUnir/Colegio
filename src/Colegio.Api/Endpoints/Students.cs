using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class StudentsEndpoints
{
    public static void MapStudentsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/students", GetAllStudents);
        app.MapGet("/api/students/{id}", GetStudentById);
        app.MapPost("/api/students", CreateStudent);
        app.MapPut("/api/students/{id}", UpdateStudent);
        app.MapDelete("/api/students/{id}", DeleteStudent);
    }

    private static async Task<IResult> GetAllStudents(ColegioDbContext db)
    {
        var students = await db.Students
            .AsNoTracking()
            .Include(s => s.Classroom)
            .ToListAsync();
        return Results.Ok(students);
    }

    private static async Task<IResult> GetStudentById(ColegioDbContext db, Guid id)
    {
        var student = await db.Students
            .AsNoTracking()
            .Include(s => s.Classroom)
            .FirstOrDefaultAsync(s => s.Id == id);
        return student is null ? Results.NotFound() : Results.Ok(student);
    }

    private static async Task<IResult> CreateStudent(ColegioDbContext db, Student student)
    {
        student.Id = Guid.NewGuid();
        db.Students.Add(student);
        await db.SaveChangesAsync();
        return Results.Created($"/api/students/{student.Id}", student);
    }

    private static async Task<IResult> UpdateStudent(ColegioDbContext db, Guid id, Student updated)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);
        if (student is null) return Results.NotFound();

        student.FirstName = updated.FirstName;
        student.LastName = updated.LastName;
        student.DateOfBirth = updated.DateOfBirth;
        student.ClassroomId = updated.ClassroomId;

        db.Students.Update(student);
        await db.SaveChangesAsync();
        return Results.Ok(student);
    }

    private static async Task<IResult> DeleteStudent(ColegioDbContext db, Guid id)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.Id == id);
        if (student is null) return Results.NotFound();

        db.Students.Remove(student);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}