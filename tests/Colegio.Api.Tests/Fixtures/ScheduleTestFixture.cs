using Colegio.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Tests.Fixtures;

public class ScheduleTestFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public ColegioDbContext Context { get; private set; }

    public ScheduleTestFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ColegioDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ColegioDbContext(options);
        Context.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Close();
        _connection.Dispose();
    }

    public async Task ResetDatabaseAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.Database.EnsureCreatedAsync();
    }
}
