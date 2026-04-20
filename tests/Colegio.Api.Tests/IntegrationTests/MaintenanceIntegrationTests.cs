using Colegio.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Colegio.Api.Tests.IntegrationTests;

public class MaintenanceIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public MaintenanceIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetStats_ShouldReturnDatabaseStatistics()
    {
        // Act
        var response = await _client.GetAsync("/api/maintenance/stats");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stats = await response.Content.ReadFromJsonAsync<DatabaseStats>();
        stats.Should().NotBeNull();
        stats!.Schools.Should().BeGreaterThanOrEqualTo(0);
        stats!.Teachers.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task ResetDatabase_ShouldSeedTestData()
    {
        // Act
        var response = await _client.PostAsync("/api/maintenance/reset", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<dynamic>();
        // Check if stats are now > 0
        var statsRes = await _client.GetAsync("/api/maintenance/stats");
        var stats = await statsRes.Content.ReadFromJsonAsync<DatabaseStats>();
        stats!.Schools.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task ClearDatabase_ShouldRemoveAllData()
    {
        // Act
        var response = await _client.DeleteAsync("/api/maintenance/clear");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        // Verify stats are 0
        var statsRes = await _client.GetAsync("/api/maintenance/stats");
        var stats = await statsRes.Content.ReadFromJsonAsync<DatabaseStats>();
        stats!.Schools.Should().Be(0);
        stats!.Teachers.Should().Be(0);
    }

    private class DatabaseStats
    {
        public int Schools { get; set; }
        public int Teachers { get; set; }
        public int Students { get; set; }
        public int Parents { get; set; }
        public int Classrooms { get; set; }
        public int Schedules { get; set; }
        public int Invoices { get; set; }
    }
}
