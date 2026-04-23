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
            .OrderBy(ts => ts.DayOfWeek)
            .ThenBy(ts => ts.StartTime)
            .ToListAsync();
        return Results.Ok(slots);
    }
}
