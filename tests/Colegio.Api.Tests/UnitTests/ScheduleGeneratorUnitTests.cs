using Colegio.Api.Tests.Fixtures;
using Colegio.Api.Tests.Helpers;
using Colegio.Domain.Entities;
using Colegio.Infrastructure.Services;
using FluentAssertions;
using DayOfWeek = Colegio.Domain.Entities.DayOfWeek;

namespace Colegio.Api.Tests.UnitTests;

public class ScheduleGeneratorUnitTests : IClassFixture<ScheduleTestFixture>
{
    private readonly ScheduleTestFixture _fixture;
    private readonly ScheduleTestDataBuilder _builder;
    private readonly ScheduleGeneratorService _sut;

    public ScheduleGeneratorUnitTests(ScheduleTestFixture fixture)
    {
        _fixture = fixture;
        _builder = new ScheduleTestDataBuilder(_fixture.Context);
        _sut = new ScheduleGeneratorService(_fixture.Context);
    }

    [Fact]
    public async Task GenerateAsync_ScenarioMinimo_DebeGenerarHorarios()
    {
        await _fixture.ResetDatabaseAsync();
        _builder.WithStandardTimeSlots();
        var teacher = _builder.CreateTeacher("Juan", "Matematicas");
        var subject = _builder.CreateSubject("Matematicas");
        var classroom = _builder.CreateClassroom(GradeLevel.Primary1, ClassroomLine.A);
        _builder.CreateClassUnit(classroom.Id, subject.Id, teacher.Id, 3);
        await _builder.SaveAsync();

        var result = await _sut.GenerateAsync(classroom.Id, AcademicSessionType.Standard);

        result.Success.Should().BeTrue();
        result.Schedules.Should().HaveCount(3);
    }

    [Fact]
    public async Task GenerateAsync_ClassroomDoubleBooking_ShouldFailIfNoSlotsAvailable()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var slot = new TimeSlot
        {
            Id = Guid.NewGuid(),
            SessionType = AcademicSessionType.Standard,
            DayOfWeek = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        };
        _fixture.Context.TimeSlots.Add(slot);
        
        var teacher1 = _builder.CreateTeacher("Profe 1", "Mates");
        var teacher2 = _builder.CreateTeacher("Profe 2", "Lengua");
        var subject1 = _builder.CreateSubject("Mates");
        var subject2 = _builder.CreateSubject("Lengua");
        var classroom = _builder.CreateClassroom(GradeLevel.Primary1, ClassroomLine.A);
        
        var lockedSchedule = new Schedule
        {
            Id = Guid.NewGuid(),
            ClassroomId = classroom.Id,
            TeacherId = teacher1.Id,
            SubjectId = subject1.Id,
            TimeSlotId = slot.Id,
            IsLocked = true
        };
        _fixture.Context.Schedules.Add(lockedSchedule);
        
        // Creamos la unidad de clase permitiendo 1 sesion (ya no hay slots libres para esta clase)
        _builder.CreateClassUnit(classroom.Id, subject2.Id, teacher2.Id, 1);
        await _builder.SaveAsync();

        // Act
        var result = await _sut.GenerateAsync(classroom.Id, AcademicSessionType.Standard);

        // Assert
        result.Success.Should().BeFalse("Ahora el motor sabe que el aula está ocupada y no puede asignar otra clase en el mismo slot");
    }

    [Fact]
    public async Task GenerateAsync_PreferredFreeDay_ShouldNowBeSoftConstraint()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        var slots = new List<TimeSlot>();
        // 2 slots el Martes
        for (int i = 1; i <= 2; i++) 
        {
            slots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = DayOfWeek.Tuesday, StartTime = new TimeSpan(8 + i, 0, 0), EndTime = new TimeSpan(9 + i, 0, 0) });
        }
        // 2 slots el Lunes
        for (int i = 1; i <= 2; i++) 
        {
            slots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = DayOfWeek.Monday, StartTime = new TimeSpan(8 + i, 0, 0), EndTime = new TimeSpan(9 + i, 0, 0) });
        }
        _fixture.Context.TimeSlots.AddRange(slots);

        var teacher = _builder.CreateTeacher("Juan", "Mates");
        teacher.PreferredFreeDay = DayOfWeek.Monday; 
        
        var subject = _builder.CreateSubject("Mates");
        var classroom = _builder.CreateClassroom(GradeLevel.Primary1, ClassroomLine.A);
        
        // Necesitamos 3 sesiones. 
        // Aumentamos MaxSessionsPerDay a 2 para que no sea el cuello de botella
        var unit = _builder.CreateClassUnit(classroom.Id, subject.Id, teacher.Id, 3);
        unit.MaxSessionsPerDay = 2; 
        await _builder.SaveAsync();

        // Act
        var result = await _sut.GenerateAsync(classroom.Id, AcademicSessionType.Standard);

        // Assert
        result.Success.Should().BeTrue("PreferredFreeDay ya no es una restricción excluyente y MaxSessionsPerDay=2 permite cubrir las 3 sesiones en 2 días");
        result.Schedules.Should().HaveCount(3);
    }

    [Fact]
    public async Task GenerateAllAsync_MultipleClassrooms_ShouldGenerateSchedulesForAll()
    {
        // Arrange
        await _fixture.ResetDatabaseAsync();
        _builder.WithStandardTimeSlots();
        
        var teacher = _builder.CreateTeacher("Maria", "Matematicas");
        var subject = _builder.CreateSubject("Matematicas");
        
        var classroom1 = _builder.CreateClassroom(GradeLevel.Primary1, ClassroomLine.A);
        var classroom2 = _builder.CreateClassroom(GradeLevel.Primary1, ClassroomLine.B);
        
        _builder.CreateClassUnit(classroom1.Id, subject.Id, teacher.Id, 2);
        _builder.CreateClassUnit(classroom2.Id, subject.Id, teacher.Id, 2);
        
        await _builder.SaveAsync();

        // Act
        var result = await _sut.GenerateAllAsync(AcademicSessionType.Standard);

        // Assert
        result.Success.Should().BeTrue();
        result.Schedules.Should().HaveCount(4); // 2 classes * 2 sessions
        
        // Ensure no teacher double booking in the generated schedules
        var schedules = result.Schedules;
        schedules.GroupBy(s => s.TimeSlotId).Any(g => g.Count() > 1).Should().BeFalse("Un profesor no puede estar en dos sitios a la vez");
    }
}
