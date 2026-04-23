using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class TimeSlotsEndpoints
{
    public static void MapTimeSlotsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/timeslots", GetAllTimeSlots);
    }

    private static async Task<IResult> GetAllTimeSlots(ColegioDbContext db)
    {
        var slots = await db.TimeSlots
            .AsNoTracking()
            .ToListAsync();
            
        var orderedSlots = slots
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.StartTime)
            .ToList();

        return Results.Ok(orderedSlots);
    }
}
