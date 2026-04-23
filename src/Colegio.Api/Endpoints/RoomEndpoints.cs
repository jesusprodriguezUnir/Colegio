using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class RoomEndpoints
{
    public static void MapRoomEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/rooms").WithTags("Rooms");

        group.MapGet("/", GetAllRooms);
        group.MapGet("/{id}", GetRoomById);
        group.MapPost("/", CreateRoom);
        group.MapPut("/{id}", UpdateRoom);
        group.MapDelete("/{id}", DeleteRoom);
        group.MapGet("/{id}/availability", GetRoomAvailability);
    }

    private static async Task<IResult> GetAllRooms(ColegioDbContext db)
    {
        var rooms = await db.Rooms.AsNoTracking().ToListAsync();
        return Results.Ok(rooms);
    }

    private static async Task<IResult> GetRoomById(ColegioDbContext db, Guid id)
    {
        var room = await db.Rooms.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        return room is null ? Results.NotFound() : Results.Ok(room);
    }

    private static async Task<IResult> CreateRoom(ColegioDbContext db, Room room)
    {
        room.Id = Guid.NewGuid();
        db.Rooms.Add(room);
        await db.SaveChangesAsync();
        return Results.Created($"/api/rooms/{room.Id}", room);
    }

    private static async Task<IResult> UpdateRoom(ColegioDbContext db, Guid id, Room updated)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room is null) return Results.NotFound();

        room.Name = updated.Name;
        room.Type = updated.Type;
        room.Capacity = updated.Capacity;
        room.Building = updated.Building;
        room.Floor = updated.Floor;
        room.Description = updated.Description;

        db.Rooms.Update(room);
        await db.SaveChangesAsync();
        return Results.Ok(room);
    }

    private static async Task<IResult> DeleteRoom(ColegioDbContext db, Guid id)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room is null) return Results.NotFound();

        db.Rooms.Remove(room);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }

    private static async Task<IResult> GetRoomAvailability(ColegioDbContext db, Guid id, AcademicSessionType sessionType)
    {
        var schedules = await db.Schedules
            .Include(s => s.TimeSlot)
            .Where(s => s.RoomId == id && s.TimeSlot.SessionType == sessionType)
            .Select(s => new { s.TimeSlotId, s.ClassroomId, s.SubjectId })
            .ToListAsync();

        return Results.Ok(schedules);
    }
}
