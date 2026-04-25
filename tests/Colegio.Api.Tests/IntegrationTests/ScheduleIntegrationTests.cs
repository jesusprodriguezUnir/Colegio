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

        var classroom = new Classroom { Id = Guid.NewGuid(), GradeLevel = GradeLevel.Primary3, Line = ClassroomLine.A, SchoolId = school.Id };
        db.Classrooms.Add(classroom);

        var slot = new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = Domain.Entities.DayOfWeek.Monday, StartTime = new TimeSpan(9,0,0), EndTime = new TimeSpan(10,0,0) };
        db.TimeSlots.Add(slot);

        db.ClassUnits.Add(new ClassUnit { Id = Guid.NewGuid(), ClassroomId = classroom.Id, SubjectId = subject.Id, TeacherId = teacher.Id, WeeklySessions = 1, IsActive = true });

        await db.SaveChangesAsync();

        var response = await _client.PostAsync($"/api/schedules/generate?classroomId={classroom.Id}&sessionType={AcademicSessionType.Standard}", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("schedules");
    }

    [Fact]
    public async Task GenerateEndpoint_ShouldReturnSchedulesInCamelCase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ColegioDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        var school = new School { Id = Guid.NewGuid(), Name = "School CamelCase" };
        db.Schools.Add(school);
        var teacher = new Teacher { Id = Guid.NewGuid(), FirstName = "Camel", MaxWorkingHours = 20 };
        db.Teachers.Add(teacher);
        var subject = new Subject { Id = Guid.NewGuid(), Name = "Mates" };
        db.Subjects.Add(subject);
        var classroom = new Classroom { Id = Guid.NewGuid(), GradeLevel = GradeLevel.Primary3, Line = ClassroomLine.A, SchoolId = school.Id };
        db.Classrooms.Add(classroom);
        var slot = new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = Domain.Entities.DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 0, 0) };
        db.TimeSlots.Add(slot);
        db.ClassUnits.Add(new ClassUnit { Id = Guid.NewGuid(), ClassroomId = classroom.Id, SubjectId = subject.Id, TeacherId = teacher.Id, WeeklySessions = 1, IsActive = true });
        await db.SaveChangesAsync();

        var response = await _client.PostAsync($"/api/schedules/generate?classroomId={classroom.Id}&sessionType=0", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"schedules\"");
        json.Should().NotContain("\"Schedules\"");
    }

    [Fact]
    public async Task GenerateEndpoint_ClassroomNotFound_ShouldReturnBadRequest()
    {
        var response = await _client.PostAsync($"/api/schedules/generate?classroomId={Guid.NewGuid()}&sessionType=0", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ScoreEndpoint_InvalidSessionType_ShouldReturnBadRequest()
    {
        var response = await _client.GetAsync("/api/schedules/score?sessionType=99");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("sessionType");
    }
}
