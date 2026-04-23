using Colegio.Api.Endpoints;
using Colegio.Domain.Services;
using Colegio.Infrastructure.Data;
using Colegio.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=colegio.db";

builder.Services.AddDbContext<ColegioDbContext>(options =>
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        sqliteOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    }));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IScheduleGenerator, ScheduleGeneratorService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ColegioDbContext>();

if (args.Contains("--migrate"))
{
    await dbContext.Database.MigrateAsync();
}
else
{
    await dbContext.Database.EnsureCreatedAsync();
}

bool forceSeed = args.Contains("--seed-force");
await SeedData.SeedAsync(dbContext, forceSeed);

if (args.Contains("--seed-only"))
{
    Console.WriteLine("Seeding completed. Exiting...");
    return;
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/health", () => Results.Ok(new { Status = "OK" }))
    .WithTags("Health");

app.MapSchoolsEndpoints();
app.MapTeachersEndpoints();
app.MapClassroomsEndpoints();
app.MapStudentsEndpoints();
app.MapParentsEndpoints();
app.MapInvoicesEndpoints();
app.MapSchedulesEndpoints();
app.MapTimeSlotsEndpoints();
app.MapCurriculumEndpoints();
app.MapRoomEndpoints();
app.MapConstraintEndpoints();
app.MapMaintenanceEndpoints();

app.Run();

public partial class Program { }