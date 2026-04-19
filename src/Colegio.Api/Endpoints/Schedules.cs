using Colegio.Domain.Entities;
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
        app.MapPut("/api/schedules/{id}", UpdateSchedule);
        app.MapDelete("/api/schedules/{id}", DeleteSchedule);
    }

    private static async Task<IResult> GetAllSchedules(ColegioDbContext db)
    {
        var schedules = await db.Schedules
            .AsNoTracking()
            .Include(s => s.Classroom)
            .Include(s => s.Teacher)
            .ToListAsync();
        return Results.Ok(schedules);
    }

    private static async Task<IResult> GetScheduleById(ColegioDbContext db, Guid id)
    {
        var schedule = await db.Schedules
            .AsNoTracking()
            .Include(s => s.Classroom)
            .Include(s => s.Teacher)
            .FirstOrDefaultAsync(s => s.Id == id);
        return schedule is null ? Results.NotFound() : Results.Ok(schedule);
    }

    private static async Task<IResult> GetSchedulesByClassroom(ColegioDbContext db, Guid classroomId)
    {
        var schedules = await db.Schedules
            .AsNoTracking()
            .Where(s => s.ClassroomId == classroomId)
            .Include(s => s.Teacher)
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

    private static async Task<IResult> UpdateSchedule(ColegioDbContext db, Guid id, Schedule updated)
    {
        var schedule = await db.Schedules.FirstOrDefaultAsync(s => s.Id == id);
        if (schedule is null) return Results.NotFound();

        schedule.ClassroomId = updated.ClassroomId;
        schedule.TeacherId = updated.TeacherId;
        schedule.Subject = updated.Subject;
        schedule.DayOfWeek = updated.DayOfWeek;
        schedule.StartTime = updated.StartTime;
        schedule.EndTime = updated.EndTime;

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