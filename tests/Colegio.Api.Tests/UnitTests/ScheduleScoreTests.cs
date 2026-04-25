using Colegio.Api.Tests.Fixtures;
using Colegio.Api.Tests.Helpers;
using Colegio.Domain.Entities;
using Colegio.Infrastructure.Services;
using FluentAssertions;
using DayOfWeek = Colegio.Domain.Entities.DayOfWeek;

namespace Colegio.Api.Tests.UnitTests;

public class ScheduleScoreTests : IClassFixture<ScheduleTestFixture>
{
    private readonly ScheduleTestFixture _fixture;
    private readonly ScheduleTestDataBuilder _builder;
    private readonly ScheduleGeneratorService _sut;

    public ScheduleScoreTests(ScheduleTestFixture fixture)
    {
        _fixture = fixture;
        _builder = new ScheduleTestDataBuilder(_fixture.Context);
        _sut = new ScheduleGeneratorService(_fixture.Context);
    }

    [Fact]
    public async Task CalculateScore_NoPreferredSlots_TeacherSatisfactionShouldBeZero()
    {
        await _fixture.ResetDatabaseAsync();
        _builder.WithStandardTimeSlots();
        var teacher = _builder.CreateTeacher("Juan", "Mates");
        var subject = _builder.CreateSubject("Mates");
        var classroom = _builder.CreateClassroom(GradeLevel.Primary3, ClassroomLine.A);
        _builder.CreateClassUnit(classroom.Id, subject.Id, teacher.Id, 2);
        await _builder.SaveAsync();

        var generated = await _sut.GenerateAsync(classroom.Id, AcademicSessionType.Standard);
        generated.Success.Should().BeTrue();
        _fixture.Context.Schedules.AddRange(generated.Schedules);
        await _fixture.Context.SaveChangesAsync();

        // No availabilities seeded → all slots are Available (not Preferred) → satisfaction = 0
        var result = await _sut.CalculateScoreAsync(AcademicSessionType.Standard);

        result.TeacherSatisfaction.Should().Be(0);
    }

    [Fact]
    public async Task CalculateScore_AllPreferredSlots_TeacherSatisfactionShouldBe100()
    {
        await _fixture.ResetDatabaseAsync();
        var teacher = _builder.CreateTeacher("Ana", "Lengua");
        var subject = _builder.CreateSubject("Lengua");
        var classroom = _builder.CreateClassroom(GradeLevel.Primary3, ClassroomLine.A);

        var slot = new TimeSlot
        {
            Id = Guid.NewGuid(),
            SessionType = AcademicSessionType.Standard,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        };
        _fixture.Context.TimeSlots.Add(slot);

        _fixture.Context.TeacherAvailabilities.Add(new TeacherAvailability
        {
            Id = Guid.NewGuid(),
            TeacherId = teacher.Id,
            TimeSlotId = slot.Id,
            IsAvailable = true,
            Level = AvailabilityLevel.Preferred
        });

        _builder.CreateClassUnit(classroom.Id, subject.Id, teacher.Id, 1);
        await _builder.SaveAsync();

        var generated = await _sut.GenerateAsync(classroom.Id, AcademicSessionType.Standard);
        generated.Success.Should().BeTrue();
        _fixture.Context.Schedules.AddRange(generated.Schedules);
        await _fixture.Context.SaveChangesAsync();

        var result = await _sut.CalculateScoreAsync(AcademicSessionType.Standard);

        result.TeacherSatisfaction.Should().Be(100);
    }

    [Fact]
    public async Task CalculateBalance_EvenDistribution_ShouldScoreHigherThanConcentrated()
    {
        await _fixture.ResetDatabaseAsync();

        var teacher = _builder.CreateTeacher("Carlos", "Mates");
        var subject = _builder.CreateSubject("Mates");
        var classroom = _builder.CreateClassroom(GradeLevel.Primary3, ClassroomLine.A);
        await _builder.SaveAsync();

        // Set A: Lengua 4 días distintos
        var slotsSpread = new[]
        {
            CreateSlot(DayOfWeek.Monday, 9),
            CreateSlot(DayOfWeek.Tuesday, 9),
            CreateSlot(DayOfWeek.Wednesday, 9),
            CreateSlot(DayOfWeek.Thursday, 9),
        };
        _fixture.Context.TimeSlots.AddRange(slotsSpread);
        await _fixture.Context.SaveChangesAsync();

        foreach (var s in slotsSpread)
            _fixture.Context.Schedules.Add(MakeSchedule(classroom.Id, teacher.Id, subject.Id, s.Id));
        await _fixture.Context.SaveChangesAsync();
        var scoreA = await _sut.CalculateScoreAsync(AcademicSessionType.Standard);
        double balanceA = scoreA.BalanceScore;

        _fixture.Context.ChangeTracker.Clear();
        var toRemove = _fixture.Context.Schedules.ToList();
        _fixture.Context.Schedules.RemoveRange(toRemove);
        await _fixture.Context.SaveChangesAsync();
        _fixture.Context.ChangeTracker.Clear();

        // Set B: Lengua 4 veces el mismo día
        var slotsConcentrated = new[]
        {
            CreateSlot(DayOfWeek.Friday, 9),
            CreateSlot(DayOfWeek.Friday, 10),
            CreateSlot(DayOfWeek.Friday, 11),
            CreateSlot(DayOfWeek.Friday, 12),
        };
        _fixture.Context.TimeSlots.AddRange(slotsConcentrated);
        await _fixture.Context.SaveChangesAsync();

        foreach (var s in slotsConcentrated)
            _fixture.Context.Schedules.Add(MakeSchedule(classroom.Id, teacher.Id, subject.Id, s.Id));
        await _fixture.Context.SaveChangesAsync();
        var scoreB = await _sut.CalculateScoreAsync(AcademicSessionType.Standard);
        double balanceB = scoreB.BalanceScore;

        balanceA.Should().BeGreaterThan(balanceB);
    }

    [Fact]
    public async Task CalculateScore_NoSchedules_ShouldReturnZeroTotal()
    {
        await _fixture.ResetDatabaseAsync();
        await _builder.SaveAsync();

        var result = await _sut.CalculateScoreAsync(AcademicSessionType.Standard);

        result.TotalScore.Should().Be(0);
    }

    private TimeSlot CreateSlot(DayOfWeek day, int hour) => new()
    {
        Id = Guid.NewGuid(),
        SessionType = AcademicSessionType.Standard,
        DayOfWeek = day,
        StartTime = new TimeSpan(hour, 0, 0),
        EndTime = new TimeSpan(hour + 1, 0, 0)
    };

    private Schedule MakeSchedule(Guid classroomId, Guid teacherId, Guid subjectId, Guid slotId) => new()
    {
        Id = Guid.NewGuid(),
        ClassroomId = classroomId,
        TeacherId = teacherId,
        SubjectId = subjectId,
        TimeSlotId = slotId
    };
}
