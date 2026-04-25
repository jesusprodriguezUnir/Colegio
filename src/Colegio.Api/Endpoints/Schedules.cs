using Colegio.Domain.Entities;
using Colegio.Domain.Services;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class SchedulesEndpoints
{
    public static void MapSchedulesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/schedules").WithTags("Schedules");

        group.MapGet("/", GetAllSchedules);
        group.MapGet("/{id}", GetScheduleById);
        group.MapGet("/classroom/{classroomId}", GetSchedulesByClassroom);
        group.MapGet("/teacher/{teacherId}", GetSchedulesByTeacher);
        group.MapGet("/room/{roomId}", GetSchedulesByRoom);
        group.MapPost("/", CreateSchedule);
        group.MapPost("/generate", GenerateSchedule);
        group.MapPost("/generate-all", GenerateAllSchedules);
        group.MapPost("/validate", ValidateSchedules);
        group.MapGet("/score", GetScheduleScore);
        group.MapGet("/debug/{classroomId}", DebugClassroomSchedule);
        group.MapPut("/{id}", UpdateSchedule);
        group.MapDelete("/{id}", DeleteSchedule);
    }

    private static async Task<IResult> GetAllSchedules(ColegioDbContext db)
    {
        var schedules = await db.Schedules
            .AsNoTracking()
            .Include(s => s.Classroom)
            .Include(s => s.Teacher)
            .Include(s => s.Subject)
            .Include(s => s.TimeSlot)
            .Include(s => s.Room)
            .ToListAsync();
        return Results.Ok(schedules);
    }

    private static async Task<IResult> GetScheduleById(ColegioDbContext db, Guid id)
    {
        var schedule = await db.Schedules
            .AsNoTracking()
            .Include(s => s.Classroom)
            .Include(s => s.Teacher)
            .Include(s => s.Subject)
            .Include(s => s.TimeSlot)
            .Include(s => s.Room)
            .FirstOrDefaultAsync(s => s.Id == id);
        return schedule is null ? Results.NotFound() : Results.Ok(schedule);
    }

    private static async Task<IResult> GetSchedulesByTeacher(ColegioDbContext db, Guid teacherId)
    {
        var schedules = await db.Schedules
            .AsNoTracking()
            .Where(s => s.TeacherId == teacherId)
            .Include(s => s.Classroom)
            .Include(s => s.Subject)
            .Include(s => s.TimeSlot)
            .Include(s => s.Room)
            .ToListAsync();
        return Results.Ok(schedules);
    }

    private static async Task<IResult> GetSchedulesByRoom(ColegioDbContext db, Guid roomId)
    {
        var schedules = await db.Schedules
            .AsNoTracking()
            .Where(s => s.RoomId == roomId)
            .Include(s => s.Classroom)
            .Include(s => s.Teacher)
            .Include(s => s.Subject)
            .Include(s => s.TimeSlot)
            .ToListAsync();
        return Results.Ok(schedules);
    }

    private static async Task<IResult> GetSchedulesByClassroom(ColegioDbContext db, Guid classroomId)
    {
        var schedules = await db.Schedules
            .AsNoTracking()
            .Where(s => s.ClassroomId == classroomId)
            .Include(s => s.Teacher)
            .Include(s => s.Subject)
            .Include(s => s.TimeSlot)
            .Include(s => s.Room)
            .ToListAsync();
        return Results.Ok(schedules);
    }

    private static async Task<IResult> CreateSchedule(ColegioDbContext db, Schedule schedule)
    {
        schedule.Id = Guid.NewGuid();
        db.Schedules.Add(schedule);
        await db.SaveChangesAsync();
        return Results.Created($"/api/schedules/{schedule.Id}", schedule);
    }

    private static async Task<IResult> GenerateSchedule(ColegioDbContext db, IScheduleGenerator generator, Guid classroomId, AcademicSessionType sessionType)
    {
        try
        {
            var toRemove = await db.Schedules
                .Where(s => s.ClassroomId == classroomId && s.TimeSlot.SessionType == sessionType && !s.IsLocked)
                .ToListAsync();
            db.Schedules.RemoveRange(toRemove);
            await db.SaveChangesAsync();

            var result = await generator.GenerateAsync(classroomId, sessionType);
            
            if (result.Success)
            {
                db.Schedules.AddRange(result.Schedules);
                await db.SaveChangesAsync();

                // Reload to get navigation properties for the UI
                var finalResult = await db.Schedules
                    .Include(s => s.Teacher)
                    .Include(s => s.Subject)
                    .Include(s => s.TimeSlot)
                    .Include(s => s.Room)
                    .Where(s => s.ClassroomId == classroomId && s.TimeSlot.SessionType == sessionType)
                    .ToListAsync();

                return Results.Ok(new { score = result.Score, warnings = result.Warnings, stats = result.Stats, schedules = finalResult });
            }
            
            return Results.BadRequest(new { Error = "No se pudo generar el horario", result.Warnings, result.Stats });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { Error = ex.Message });
        }
    }

    private static async Task<IResult> GenerateAllSchedules(ColegioDbContext db, IScheduleGenerator generator, AcademicSessionType sessionType)
    {
        try
        {
            var toRemove = await db.Schedules
                .Where(s => s.TimeSlot.SessionType == sessionType && !s.IsLocked)
                .ToListAsync();
            db.Schedules.RemoveRange(toRemove);
            await db.SaveChangesAsync();

            var result = await generator.GenerateAllAsync(sessionType);
            
            return Results.Ok(new { Count = result.Schedules.Count, result.Score, result.Warnings, result.Stats, Message = "Schedules generated for all classrooms" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { Error = ex.Message });
        }
    }

    private static async Task<IResult> ValidateSchedules(IScheduleGenerator generator, AcademicSessionType sessionType)
    {
        var result = await generator.ValidateAsync(sessionType);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetScheduleScore(IScheduleGenerator generator, AcademicSessionType sessionType)
    {
        if (!Enum.IsDefined(typeof(AcademicSessionType), sessionType))
            return Results.BadRequest(new { error = "sessionType inválido. Usar 0 (Standard) o 1 (Intensive)." });

        var result = await generator.CalculateScoreAsync(sessionType);
        return Results.Ok(result);
    }

    private static async Task<IResult> DebugClassroomSchedule(IScheduleGenerator generator, Guid classroomId, AcademicSessionType sessionType)
    {
        var result = await generator.DebugConstraintsAsync(classroomId, sessionType);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateSchedule(ColegioDbContext db, Guid id, Schedule updated)
    {
        var schedule = await db.Schedules.FirstOrDefaultAsync(s => s.Id == id);
        if (schedule is null) return Results.NotFound();

        schedule.ClassroomId = updated.ClassroomId;
        schedule.TeacherId = updated.TeacherId;
        schedule.SubjectId = updated.SubjectId;
        schedule.TimeSlotId = updated.TimeSlotId;
        schedule.IsLocked = updated.IsLocked;
        schedule.Type = updated.Type;
        schedule.RoomId = updated.RoomId;

        db.Schedules.Update(schedule);
        await db.SaveChangesAsync();
        return Results.Ok(schedule);
    }

    private static async Task<IResult> DeleteSchedule(ColegioDbContext db, Guid id)
    {
        var schedule = await db.Schedules.FirstOrDefaultAsync(s => s.Id == id);
        if (schedule is null) return Results.NotFound();

        db.Schedules.Remove(schedule);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}