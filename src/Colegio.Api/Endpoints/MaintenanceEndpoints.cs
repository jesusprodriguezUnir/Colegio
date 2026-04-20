using Colegio.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Colegio.Api.Endpoints;

public static class MaintenanceEndpoints
{
    public static void MapMaintenanceEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/maintenance")
            .WithTags("Maintenance");

        group.MapGet("/stats", async (ColegioDbContext db) =>
        {
            var stats = await SeedData.GetDatabaseStatsAsync(db);
            return Results.Ok(stats);
        })
        .WithName("GetDatabaseStats")
        .WithOpenApi();

        group.MapPost("/reset", async (ColegioDbContext db) =>
        {
            await SeedData.SeedAsync(db, force: true);
            return Results.Ok(new { Message = "Base de datos reiniciada con éxito" });
        })
        .WithName("ResetDatabase")
        .WithOpenApi();

        group.MapDelete("/clear", async (ColegioDbContext db) =>
        {
            await SeedData.ClearAllDataAsync(db);
            return Results.Ok(new { Message = "Base de datos limpiada con éxito" });
        })
        .WithName("ClearDatabase")
        .WithOpenApi();
    }
}
