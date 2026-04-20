using Colegio.Api.Endpoints;
using Colegio.Infrastructure.Data;
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

var app = builder.Build();

using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<ColegioDbContext>();
await dbContext.Database.EnsureCreatedAsync();
await SeedData.SeedAsync(dbContext);

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
app.MapMaintenanceEndpoints();

app.Run();

public partial class Program { }