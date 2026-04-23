using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class ClassUnitEndpoints
{
    public static void MapClassUnitEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/classunits").WithTags("ClassUnits");

        group.MapGet("/", GetAll);
        group.MapGet("/{id:guid}", GetById);
        group.MapGet("/classroom/{classroomId:guid}", GetByClassroom);
        group.MapPost("/", Create);
        group.MapPost("/generate-from-curriculum", GenerateFromCurriculum);
        group.MapPut("/{id:guid}", Update);
        group.MapDelete("/{id:guid}", Delete);
    }

    private static async Task<Ok<List<object>>> GetAll(ColegioDbContext db)
    {
        var units = await db.ClassUnits
            .Include(cu => cu.Classroom)
            .Include(cu => cu.Subject)
            .Include(cu => cu.Teacher)
            .Include(cu => cu.PreferredRoom)
            .Select(cu => new
            {
                cu.Id,
                cu.ClassroomId,
                ClassroomName = cu.Classroom.GradeLevel.ToString() + " " + cu.Classroom.Line.ToString(),
                GradeLevel = (int)cu.Classroom.GradeLevel,
                Line = cu.Classroom.Line.ToString(),
                cu.SubjectId,
                SubjectName = cu.Subject.Name,
                SubjectColor = cu.Subject.Color,
                cu.TeacherId,
                TeacherName = cu.Teacher != null ? cu.Teacher.FirstName + " " + cu.Teacher.LastName : null,
                cu.WeeklySessions,
                cu.SessionDuration,
                cu.AllowConsecutiveDays,
                cu.PreferNonConsecutive,
                cu.AllowDoubleSession,
                cu.MaxSessionsPerDay,
                cu.PreferredRoomId,
                PreferredRoomName = cu.PreferredRoom != null ? cu.PreferredRoom.Name : null,
                cu.SimultaneousGroupId,
                cu.IsActive
            })
            .ToListAsync();

        return TypedResults.Ok(units.Cast<object>().ToList());
    }

    private static async Task<Results<Ok<object>, NotFound>> GetById(Guid id, ColegioDbContext db)
    {
        var cu = await db.ClassUnits
            .Include(c => c.Classroom)
            .Include(c => c.Subject)
            .Include(c => c.Teacher)
            .Include(c => c.PreferredRoom)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cu is null) return TypedResults.NotFound();

        return TypedResults.Ok<object>(new
        {
            cu.Id,
            cu.ClassroomId,
            ClassroomName = cu.Classroom.GradeLevel.ToString() + " " + cu.Classroom.Line.ToString(),
            cu.SubjectId,
            SubjectName = cu.Subject.Name,
            SubjectColor = cu.Subject.Color,
            cu.TeacherId,
            TeacherName = cu.Teacher != null ? cu.Teacher.FirstName + " " + cu.Teacher.LastName : null,
            cu.WeeklySessions,
            cu.SessionDuration,
            cu.AllowConsecutiveDays,
            cu.PreferNonConsecutive,
            cu.AllowDoubleSession,
            cu.MaxSessionsPerDay,
            cu.PreferredRoomId,
            PreferredRoomName = cu.PreferredRoom != null ? cu.PreferredRoom.Name : null,
            cu.SimultaneousGroupId,
            cu.IsActive
        });
    }

    private static async Task<Ok<List<object>>> GetByClassroom(Guid classroomId, ColegioDbContext db)
    {
        var units = await db.ClassUnits
            .Include(cu => cu.Subject)
            .Include(cu => cu.Teacher)
            .Where(cu => cu.ClassroomId == classroomId)
            .Select(cu => new
            {
                cu.Id,
                cu.ClassroomId,
                cu.SubjectId,
                SubjectName = cu.Subject.Name,
                SubjectColor = cu.Subject.Color,
                cu.TeacherId,
                TeacherName = cu.Teacher != null ? cu.Teacher.FirstName + " " + cu.Teacher.LastName : null,
                cu.WeeklySessions,
                cu.SessionDuration,
                cu.AllowDoubleSession,
                cu.PreferNonConsecutive,
                cu.MaxSessionsPerDay,
                cu.IsActive
            })
            .ToListAsync();

        return TypedResults.Ok(units.Cast<object>().ToList());
    }

    private static async Task<Created<ClassUnit>> Create(ClassUnit classUnit, ColegioDbContext db)
    {
        classUnit.Id = Guid.NewGuid();
        db.ClassUnits.Add(classUnit);
        await db.SaveChangesAsync();
        return TypedResults.Created($"/api/classunits/{classUnit.Id}", classUnit);
    }

    private static async Task<Results<Ok<object>, NotFound>> Update(Guid id, ClassUnit input, ColegioDbContext db)
    {
        var existing = await db.ClassUnits.FindAsync(id);
        if (existing is null) return TypedResults.NotFound();

        db.Attach(existing);
        existing.TeacherId = input.TeacherId;
        existing.WeeklySessions = input.WeeklySessions;
        existing.SessionDuration = input.SessionDuration;
        existing.AllowConsecutiveDays = input.AllowConsecutiveDays;
        existing.PreferNonConsecutive = input.PreferNonConsecutive;
        existing.AllowDoubleSession = input.AllowDoubleSession;
        existing.MaxSessionsPerDay = input.MaxSessionsPerDay;
        existing.PreferredRoomId = input.PreferredRoomId;
        existing.SimultaneousGroupId = input.SimultaneousGroupId;
        existing.IsActive = input.IsActive;

        await db.SaveChangesAsync();
        return TypedResults.Ok<object>(new { Message = "Updated", existing.Id });
    }

    private static async Task<Results<NoContent, NotFound>> Delete(Guid id, ColegioDbContext db)
    {
        var existing = await db.ClassUnits.FindAsync(id);
        if (existing is null) return TypedResults.NotFound();
        db.ClassUnits.Remove(existing);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Generates ClassUnits for all classrooms from their grade's Curriculum.
    /// If a classroom already has ClassUnits, it is skipped.
    /// </summary>
    private static async Task<Ok<object>> GenerateFromCurriculum(ColegioDbContext db)
    {
        var curriculums = await db.Curriculums.Include(c => c.Subject).ToListAsync();
        var classrooms = await db.Classrooms.ToListAsync();
        var existingUnits = await db.ClassUnits.ToListAsync();
        var teachers = await db.Teachers.Include(t => t.Subjects).ToListAsync();
        var random = new Random(42);

        var newUnits = new List<ClassUnit>();

        // Only generate for Primary3+ (skip Infantil)
        var eligible = classrooms
            .Where(c => c.GradeLevel >= GradeLevel.Primary3)
            .Where(c => !existingUnits.Any(eu => eu.ClassroomId == c.Id))
            .ToList();

        foreach (var classroom in eligible)
        {
            var gradeCurriculum = curriculums
                .Where(c => c.GradeLevel == classroom.GradeLevel)
                .ToList();

            foreach (var entry in gradeCurriculum)
            {
                var competentTeacher = teachers
                    .Where(t => t.Subjects.Any(s => s.Id == entry.SubjectId))
                    .OrderBy(_ => random.Next())
                    .FirstOrDefault();

                newUnits.Add(new ClassUnit
                {
                    Id = Guid.NewGuid(),
                    ClassroomId = classroom.Id,
                    SubjectId = entry.SubjectId,
                    TeacherId = competentTeacher?.Id,
                    WeeklySessions = entry.WeeklyHours,
                    SessionDuration = 1,
                    AllowConsecutiveDays = true,
                    PreferNonConsecutive = entry.WeeklyHours <= 3,
                    AllowDoubleSession = entry.WeeklyHours >= 4,
                    MaxSessionsPerDay = entry.WeeklyHours >= 6 ? 2 : 1,
                    PreferredRoomId = entry.Subject.RequiredRoomId,
                    IsActive = true
                });
            }
        }

        db.ClassUnits.AddRange(newUnits);
        await db.SaveChangesAsync();

        return TypedResults.Ok<object>(new
        {
            Message = "ClassUnits generated from curriculum",
            ClassroomsProcessed = eligible.Count,
            UnitsCreated = newUnits.Count
        });
    }
}
