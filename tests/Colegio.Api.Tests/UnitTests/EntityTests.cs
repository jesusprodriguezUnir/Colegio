using Colegio.Domain.Entities;
using FluentAssertions;

namespace Colegio.Api.Tests.UnitTests;

public class EntityTests
{
    [Fact]
    public void School_ShouldHaveDefaultCollections()
    {
        var school = new School();

        school.Classrooms.Should().NotBeNull();
        school.Classrooms.Should().BeEmpty();
    }

    [Fact]
    public void Teacher_ShouldHaveDefaultCollections()
    {
        var teacher = new Teacher();

        teacher.TutoredClassrooms.Should().NotBeNull();
        teacher.TutoredClassrooms.Should().BeEmpty();
        teacher.Schedules.Should().NotBeNull();
        teacher.Schedules.Should().BeEmpty();
    }

    [Fact]
    public void Student_ShouldHaveDefaultCollections()
    {
        var student = new Student();

        student.StudentParents.Should().NotBeNull();
        student.StudentParents.Should().BeEmpty();
        student.Invoices.Should().NotBeNull();
        student.Invoices.Should().BeEmpty();
    }

    [Fact]
    public void Parent_ShouldHaveDefaultCollections()
    {
        var parent = new Parent();

        parent.StudentParents.Should().NotBeNull();
        parent.StudentParents.Should().BeEmpty();
        parent.Invoices.Should().NotBeNull();
        parent.Invoices.Should().BeEmpty();
    }

    [Fact]
    public void Classroom_ShouldInitializePropertiesCorrectly()
    {
        var classroom = new Classroom
        {
            Id = Guid.NewGuid(),
            GradeLevel = GradeLevel.Primary1,
            Line = ClassroomLine.A,
            SchoolId = Guid.NewGuid()
        };

        classroom.Students.Should().NotBeNull();
        classroom.Schedules.Should().NotBeNull();
    }

    [Fact]
    public void Invoice_ShouldInitializeWithCorrectDefaultValues()
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ParentId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            IssueDate = DateTime.UtcNow,
            DueDate = DateTime.UtcNow.AddDays(30),
            TotalAmount = 350.00m,
            Concept = InvoiceConcept.Monthly
        };

        invoice.Status.Should().Be(InvoiceStatus.Pending);
    }

    [Fact]
    public void StudentParent_ShouldHaveCorrectRelationship()
    {
        var studentParent = new StudentParent
        {
            StudentId = Guid.NewGuid(),
            ParentId = Guid.NewGuid(),
            Relationship = RelationshipType.Father
        };

        studentParent.Relationship.Should().Be(RelationshipType.Father);
    }

    [Fact]
    public void Schedule_ShouldHaveCorrectProperties()
    {
        var subjectId = Guid.NewGuid();
        var timeSlotId = Guid.NewGuid();

        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            ClassroomId = Guid.NewGuid(),
            TeacherId = Guid.NewGuid(),
            SubjectId = subjectId,
            TimeSlotId = timeSlotId,
            IsLocked = false,
            Type = ScheduleType.ClassUnit
        };

        schedule.SubjectId.Should().Be(subjectId);
        schedule.TimeSlotId.Should().Be(timeSlotId);
        schedule.IsLocked.Should().BeFalse();
        schedule.Type.Should().Be(ScheduleType.ClassUnit);
    }
}