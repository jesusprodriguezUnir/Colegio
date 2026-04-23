using Colegio.Domain.Entities;
using Colegio.Domain.Services;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class SchedulesEndpoints
{
    public static void MapSchedulesEndpoints(this WebApplication app)
    {
        app.MapGet("/api/schedules", GetAllSchedules);
        app.MapGet("/api/schedules/{id}", GetScheduleById);
        app.MapGet("/api/schedules/classroom/{classroomId}", GetSchedulesByClassroom);
        app.MapPost("/api/schedules", CreateSchedule);
        app.MapPost("/api/schedules/generate", GenerateSchedule);
        app.MapPost("/api/schedules/generate-all", GenerateAllSchedules);
        app.MapPut("/api/schedules/{id}", UpdateSchedule);
        app.MapDelete("/api/schedules/{id}", DeleteSchedule);
    }

    private static async Task<IResult> GetAllSchedules(ColegioDbContext db)
    {
        var schedules = await db.Schedules
            .AsNoTracking()
            .Include(s => s.Classroom)
            .Include(s => s.Teacher)
            .Include(s => s.Subject)
            .Include(s => s.TimeSlot)
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
            .FirstOrDefaultAsync(s => s.Id == id);
        return schedule is null ? Results.NotFound() : Results.Ok(schedule);
    }

    private static async Task<IResult> GetSchedulesByClassroom(ColegioDbContext db, Guid classroomId)
    {
        var schedules = await db.Schedules
            .AsNoTracking()
            .Where(s => s.ClassroomId == classroomId)
            .Include(s => s.Teacher)
            .Include(s => s.Subject)
            .Include(s => s.TimeSlot)
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
            // Remove existing non-locked schedules for this classroom and session type
            var toRemove = await db.Schedules
                .Include(s => s.TimeSlot)
                .Where(s => s.ClassroomId == classroomId && s.TimeSlot.SessionType == sessionType && !s.IsLocked)
                .ToListAsync();
            
            db.Schedules.RemoveRange(toRemove);
            await db.SaveChangesAsync();

            var result = await generator.GenerateAsync(classroomId, sessionType);
            
            // Only add schedules that are not already in the database
            var existingIds = await db.Schedules.Select(s => s.Id).ToListAsync();
            var newSchedules = result.Where(r => !existingIds.Contains(r.Id)).ToList();
            
            db.Schedules.AddRange(newSchedules);
            await db.SaveChangesAsync();

            // Reload to get navigation properties for the UI
            var finalResult = await db.Schedules
                .Include(s => s.Teacher)
                .Include(s => s.Subject)
                .Include(s => s.TimeSlot)
                .Where(s => s.ClassroomId == classroomId && s.TimeSlot.SessionType == sessionType)
                .ToListAsync();

            return Results.Ok(finalResult);
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
            // Remove ALL existing non-locked schedules for this session type
            var toRemove = await db.Schedules
                .Include(s => s.TimeSlot)
                .Where(s => s.TimeSlot.SessionType == sessionType && !s.IsLocked)
                .ToListAsync();
            
            db.Schedules.RemoveRange(toRemove);
            await db.SaveChangesAsync();

            var result = await generator.GenerateAllAsync(sessionType);
            
            db.Schedules.AddRange(result);
            await db.SaveChangesAsync();

            return Results.Ok(new { Count = result.Count, Message = "Schedules generated for all classrooms" });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { Error = ex.Message });
        }
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