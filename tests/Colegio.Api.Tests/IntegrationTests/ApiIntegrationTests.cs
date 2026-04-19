using Colegio.Api.Endpoints;
using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;

namespace Colegio.Api.Tests.IntegrationTests;

public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetAllSchools_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/schools");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadFromJsonAsync<List<School>>();
        content.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateSchool_ShouldReturnCreated()
    {
        var school = new School
        {
            Name = "Test School",
            Address = "Test Address",
            ContactPhone = "123456789",
            ContactEmail = "test@school.com"
        };

        var response = await _client.PostAsJsonAsync("/api/schools", school);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateAndGetSchool_ShouldReturnSchool()
    {
        var school = new School
        {
            Name = "Integration Test School",
            Address = "Address 123",
            ContactPhone = "987654321",
            ContactEmail = "test@integration.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/schools", school);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<School>();
        created!.Id.Should().NotBeEmpty();

        var getResponse = await _client.GetAsync("/api/schools");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var schools = await getResponse.Content.ReadFromJsonAsync<List<School>>();
        schools.Should().NotBeEmpty();
        schools!.First(s => s.Name == "Integration Test School").Should().NotBeNull();
    }

    [Fact]
    public async Task GetNonExistentSchool_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/schools/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndDeleteSchool_ShouldReturnNoContent()
    {
        var school = new School
        {
            Name = "School To Delete",
            Address = "Address",
            ContactPhone = "123",
            ContactEmail = "delete@test.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/schools", school);
        var created = await createResponse.Content.ReadFromJsonAsync<School>();

        var deleteResponse = await _client.DeleteAsync($"/api/schools/{created!.Id}");

        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task CreateAndUpdateSchool_ShouldReturnUpdated()
    {
        var school = new School
        {
            Name = "Original Name",
            Address = "Original Address",
            ContactPhone = "111",
            ContactEmail = "original@test.com"
        };

        var createResponse = await _client.PostAsJsonAsync("/api/schools", school);
        var created = await createResponse.Content.ReadFromJsonAsync<School>();

        created!.Name = "Updated Name";
        var updateResponse = await _client.PutAsJsonAsync($"/api/schools/{created.Id}", created);

        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await updateResponse.Content.ReadFromJsonAsync<School>();
        updated!.Name.Should().Be("Updated Name");
    }
}