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
    public void Schedule_ShouldHaveCorrectTimeProperties()
    {
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            ClassroomId = Guid.NewGuid(),
            TeacherId = Guid.NewGuid(),
            Subject = "Mathematics",
            DayOfWeek = Domain.Entities.DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        };

        schedule.StartTime.Should().Be(new TimeSpan(9, 0, 0));
        schedule.EndTime.Should().Be(new TimeSpan(10, 0, 0));
    }
}