using Colegio.Api.Endpoints;
using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Colegio.Api.Tests.IntegrationTests;

public class ScheduleIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public ScheduleIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GenerateEndpoint_ShouldReturnSchedules()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ColegioDbContext>();
        
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        var school = new School { Id = Guid.NewGuid(), Name = "Integracion School" };
        db.Schools.Add(school);

        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "Profe API", MaxWorkingHours = 20 };
        db.Teachers.Add(teacher);

        var subject = new Subject { Id = Guid.NewGuid(), Name = "Mates API" };
        db.Subjects.Add(subject);

        var classroom = new Classroom { Id = Guid.NewGuid(), GradeLevel = GradeLevel.ESO1, Line = ClassroomLine.A, SchoolId = school.Id };
        db.Classrooms.Add(classroom);

        var slot = new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = Domain.Entities.DayOfWeek.Monday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(10,0,0) };
        db.TimeSlots.Add(slot);

        db.ClassUnits.Add(new ClassUnit { Id = Guid.NewGuid(), ClassroomId = classroom.Id, SubjectId = subject.Id, TeacherId = teacher.Id, WeeklySessions = 1, IsActive = true });
        
        await db.SaveChangesAsync();

        // Act
        var response = await _client.PostAsync($"/api/schedules/generate?classroomId={classroom.Id}&sessionType={AcademicSessionType.Standard}", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("schedules"); // API returns JSON keys in camelCase usually
    }
}
