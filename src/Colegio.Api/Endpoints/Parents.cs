using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class ParentsEndpoints
{
    public static void MapParentsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/parents", GetAllParents);
        app.MapGet("/api/parents/{id}", GetParentById);
        app.MapPost("/api/parents", CreateParent);
        app.MapPut("/api/parents/{id}", UpdateParent);
        app.MapDelete("/api/parents/{id}", DeleteParent);
    }

    private static async Task<IResult> GetAllParents(ColegioDbContext db)
    {
        var parents = await db.Parents.AsNoTracking().ToListAsync();
        return Results.Ok(parents);
    }

    private static async Task<IResult> GetParentById(ColegioDbContext db, Guid id)
    {
        var parent = await db.Parents.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return parent is null ? Results.NotFound() : Results.Ok(parent);
    }

    private static async Task<IResult> CreateParent(ColegioDbContext db, Parent parent)
    {
        parent.Id = Guid.NewGuid();
        db.Parents.Add(parent);
        await db.SaveChangesAsync();
        return Results.Created($"/api/parents/{parent.Id}", parent);
    }

    private static async Task<IResult> UpdateParent(ColegioDbContext db, Guid id, Parent updated)
    {
        var parent = await db.Parents.FirstOrDefaultAsync(p => p.Id == id);
        if (parent is null) return Results.NotFound();

        parent.FirstName = updated.FirstName;
        parent.LastName = updated.LastName;
        parent.Email = updated.Email;
        parent.Phone = updated.Phone;
        parent.Address = updated.Address;

        db.Parents.Update(parent);
        await db.SaveChangesAsync();
        return Results.Ok(parent);
    }

    private static async Task<IResult> DeleteParent(ColegioDbContext db, Guid id)
    {
        var parent = await db.Parents.FirstOrDefaultAsync(p => p.Id == id);
        if (parent is null) return Results.NotFound();

        db.Parents.Remove(parent);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}