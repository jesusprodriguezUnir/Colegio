using Colegio.Api.Endpoints;
using Colegio.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Colegio.Api.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static readonly string DbName = Guid.NewGuid().ToString();
    private static readonly InMemoryDatabaseRoot DbRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ColegioDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<ColegioDbContext>(options =>
                options.UseInMemoryDatabase(DbName, DbRoot));
        });
    }
}