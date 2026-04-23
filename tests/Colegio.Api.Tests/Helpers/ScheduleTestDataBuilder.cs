using Colegio.Domain.Entities;
using Colegio.Infrastructure.Data;
using DayOfWeek = Colegio.Domain.Entities.DayOfWeek;

namespace Colegio.Api.Tests.Helpers;

public class ScheduleTestDataBuilder
{
    private readonly ColegioDbContext _context;
    private readonly List<TimeSlot> _timeSlots = new();
    private readonly List<Teacher> _teachers = new();
    private readonly List<Subject> _subjects = new();
    private readonly List<Classroom> _classrooms = new();
    private readonly School _school;

    public ScheduleTestDataBuilder(ColegioDbContext context)
    {
        _context = context;
        _school = new School
        {
            Id = Guid.NewGuid(),
            Name = "Test School"
        };
        _context.Schools.Add(_school);
    }

    public ScheduleTestDataBuilder WithStandardTimeSlots()
    {
        var days = Enum.GetValues<DayOfWeek>();
        foreach (var day in days)
        {
            for (int i = 1; i <= 6; i++)
            {
                _timeSlots.Add(new TimeSlot
                {
                    Id = Guid.NewGuid(),
                    SessionType = AcademicSessionType.Standard,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(8 + i, 0, 0),
                    EndTime = new TimeSpan(9 + i, 0, 0),
                    Label = $"{i}ª Hora"
                });
            }
        }
        _context.TimeSlots.AddRange(_timeSlots);
        return this;
    }

    public Teacher CreateTeacher(string name, string specialty)
    {
        var teacher = new Teacher
        {
            Id = Guid.NewGuid(),
            FirstName = name,
            Specialty = specialty,
            MaxWorkingHours = 25
        };
        _teachers.Add(teacher);
        _context.Teachers.Add(teacher);
        return teacher;
    }

    public Subject CreateSubject(string name)
    {
        var subject = new Subject
        {
            Id = Guid.NewGuid(),
            Name = name
        };
        _subjects.Add(subject);
        _context.Subjects.Add(subject);
        return subject;
    }

    public Classroom CreateClassroom(GradeLevel grade, ClassroomLine line)
    {
        var classroom = new Classroom
        {
            Id = Guid.NewGuid(),
            GradeLevel = grade,
            Line = line,
            SchoolId = _school.Id
        };
        _classrooms.Add(classroom);
        _context.Classrooms.Add(classroom);
        return classroom;
    }

    public ClassUnit CreateClassUnit(Guid classroomId, Guid subjectId, Guid? teacherId, int weeklySessions)
    {
        var unit = new ClassUnit
        {
            Id = Guid.NewGuid(),
            ClassroomId = classroomId,
            SubjectId = subjectId,
            TeacherId = teacherId,
            WeeklySessions = weeklySessions,
            IsActive = true,
            MaxSessionsPerDay = 1
        };
        _context.ClassUnits.Add(unit);
        return unit;
    }

    public async Task SaveAsync()
    {
        await _context.SaveChangesAsync();
    }
}
