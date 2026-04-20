using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class SchoolsEndpoints
{
    public static void MapSchoolsEndpoints(this WebApplication app)
    {
        app.MapGet("/api/schools", GetAllSchools);
        app.MapGet("/api/schools/{id:guid}", GetSchoolById);
        app.MapPost("/api/schools", CreateSchool);
        app.MapPut("/api/schools/{id:guid}", UpdateSchool);
        app.MapDelete("/api/schools/{id:guid}", DeleteSchool);
    }

    private static async Task<IResult> GetAllSchools(ColegioDbContext db)
    {
        var schools = await db.Schools.AsNoTracking().ToListAsync();
        return Results.Ok(schools);
    }

    private static async Task<IResult> GetSchoolById(ColegioDbContext db, Guid id)
    {
        var school = await db.Schools.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        return school is null ? Results.NotFound() : Results.Ok(school);
    }

    private static async Task<IResult> CreateSchool(ColegioDbContext db, School input)
    {
        var school = new School
        {
            Id = Guid.NewGuid(),
            Name = input.Name,
            CIF = input.CIF,
            Address = input.Address,
            City = input.City,
            PostalCode = input.PostalCode,
            Province = input.Province,
            ContactPhone = input.ContactPhone,
            ContactEmail = input.ContactEmail
        };
        db.Schools.Add(school);
        await db.SaveChangesAsync();
        return Results.Created($"/api/schools/{school.Id}", school);
    }

    private static async Task<IResult> UpdateSchool(ColegioDbContext db, Guid id, School updated)
    {
        var school = await db.Schools.FirstOrDefaultAsync(s => s.Id == id);
        if (school is null) return Results.NotFound();

        school.Name = updated.Name;
        school.CIF = updated.CIF;
        school.Address = updated.Address;
        school.City = updated.City;
        school.PostalCode = updated.PostalCode;
        school.Province = updated.Province;
        school.ContactPhone = updated.ContactPhone;
        school.ContactEmail = updated.ContactEmail;

        db.Schools.Update(school);
        await db.SaveChangesAsync();
        return Results.Ok(school);
    }

    private static async Task<IResult> DeleteSchool(ColegioDbContext db, Guid id)
    {
        var school = await db.Schools.FirstOrDefaultAsync(s => s.Id == id);
        if (school is null) return Results.NotFound();

        db.Schools.Remove(school);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}