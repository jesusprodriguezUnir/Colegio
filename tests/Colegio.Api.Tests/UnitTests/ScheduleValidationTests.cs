using Colegio.Api.Tests.Fixtures;
using Colegio.Api.Tests.Helpers;
using Colegio.Domain.Entities;
using Colegio.Infrastructure.Services;
using FluentAssertions;
using DayOfWeek = Colegio.Domain.Entities.DayOfWeek;

namespace Colegio.Api.Tests.UnitTests;

public class ScheduleValidationTests : IClassFixture<ScheduleTestFixture>
{
    private readonly ScheduleTestFixture _fixture;
    private readonly ScheduleTestDataBuilder _builder;
    private readonly ScheduleGeneratorService _sut;

    public ScheduleValidationTests(ScheduleTestFixture fixture)
    {
        _fixture = fixture;
        _builder = new ScheduleTestDataBuilder(_fixture.Context);
        _sut = new ScheduleGeneratorService(_fixture.Context);
    }

    [Fact]
    public async Task ValidateAsync_TeacherDoubleBooked_ShouldDetectConflict()
    {
        await _fixture.ResetDatabaseAsync();
        var teacher = _builder.CreateTeacher("Conflicto", "Mates");
        var subject = _builder.CreateSubject("Mates");
        var classroom1 = _builder.CreateClassroom(GradeLevel.Primary3, ClassroomLine.A);
        var classroom2 = _builder.CreateClassroom(GradeLevel.Primary3, ClassroomLine.B);

        var slot = new TimeSlot
        {
            Id = Guid.NewGuid(),
            SessionType = AcademicSessionType.Standard,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        };
        _fixture.Context.TimeSlots.Add(slot);

        _fixture.Context.Schedules.AddRange(
            new Schedule { Id = Guid.NewGuid(), ClassroomId = classroom1.Id, TeacherId = teacher.Id, SubjectId = subject.Id, TimeSlotId = slot.Id },
            new Schedule { Id = Guid.NewGuid(), ClassroomId = classroom2.Id, TeacherId = teacher.Id, SubjectId = subject.Id, TimeSlotId = slot.Id }
        );
        await _builder.SaveAsync();

        var result = await _sut.ValidateAsync(AcademicSessionType.Standard);

        result.IsValid.Should().BeFalse();
        result.Conflicts.Should().Contain(c => c.Type == "TeacherConflict");
    }

    [Fact]
    public async Task ValidateAsync_RoomDoubleBooked_ShouldDetectConflict()
    {
        await _fixture.ResetDatabaseAsync();
        var teacher1 = _builder.CreateTeacher("Profe1", "Mates");
        var teacher2 = _builder.CreateTeacher("Profe2", "Lengua");
        var subject = _builder.CreateSubject("Mates");
        var classroom1 = _builder.CreateClassroom(GradeLevel.Primary3, ClassroomLine.A);
        var classroom2 = _builder.CreateClassroom(GradeLevel.Primary3, ClassroomLine.B);

        var room = new Room { Id = Guid.NewGuid(), Name = "Lab", Type = RoomType.Specific, Capacity = 30 };
        _fixture.Context.Rooms.Add(room);

        var slot = new TimeSlot
        {
            Id = Guid.NewGuid(),
            SessionType = AcademicSessionType.Standard,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        };
        _fixture.Context.TimeSlots.Add(slot);

        _fixture.Context.Schedules.AddRange(
            new Schedule { Id = Guid.NewGuid(), ClassroomId = classroom1.Id, TeacherId = teacher1.Id, SubjectId = subject.Id, TimeSlotId = slot.Id, RoomId = room.Id },
            new Schedule { Id = Guid.NewGuid(), ClassroomId = classroom2.Id, TeacherId = teacher2.Id, SubjectId = subject.Id, TimeSlotId = slot.Id, RoomId = room.Id }
        );
        await _builder.SaveAsync();

        var result = await _sut.ValidateAsync(AcademicSessionType.Standard);

        result.IsValid.Should().BeFalse();
        result.Conflicts.Should().Contain(c => c.Type == "RoomConflict");
    }

    [Fact]
    public async Task ValidateAsync_ValidSchedule_ShouldReturnIsValidTrue()
    {
        await _fixture.ResetDatabaseAsync();
        _builder.WithStandardTimeSlots();
        var teacher = _builder.CreateTeacher("Limpio", "Mates");
        var subject = _builder.CreateSubject("Mates");
        var classroom = _builder.CreateClassroom(GradeLevel.Primary3, ClassroomLine.A);
        _builder.CreateClassUnit(classroom.Id, subject.Id, teacher.Id, 2);
        await _builder.SaveAsync();

        await _sut.GenerateAsync(classroom.Id, AcademicSessionType.Standard);

        var result = await _sut.ValidateAsync(AcademicSessionType.Standard);

        result.IsValid.Should().BeTrue();
        result.Conflicts.Should().NotContain(c => c.Severity == "Error");
    }
}
