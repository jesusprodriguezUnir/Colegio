using Colegio.Domain.Entities;

namespace Colegio.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ColegioDbContext context)
    {
        if (context.Schools.Any()) return;

        var school = new School
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Colegio Público Virgen del Carmen",
            Address = "Calle Mayor 123, Madrid",
            ContactPhone = "912345678",
            ContactEmail = "colegio@virgendelcarmen.es"
        };
        context.Schools.Add(school);

        var teachers = new List<Teacher>
        {
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222221111"), FirstName = "María", LastName = "García López", Specialty = "Educación Infantil", HireDate = new DateTime(2018, 9, 1) },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222221112"), FirstName = "José", LastName = "Martínez Sánchez", Specialty = "Matemáticas", HireDate = new DateTime(2015, 9, 1) },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222221113"), FirstName = "Ana", LastName = "Rodríguez Torres", Specialty = "Lengua", HireDate = new DateTime(2016, 9, 1) },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222221114"), FirstName = "Carlos", LastName = "López Fernández", Specialty = "Ciencias", HireDate = new DateTime(2017, 9, 1) },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222221115"), FirstName = "Laura", LastName = "González Ruiz", Specialty = "Educación Física", HireDate = new DateTime(2019, 9, 1) },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222221116"), FirstName = "David", LastName = "Pérez Gómez", Specialty = "Inglés", HireDate = new DateTime(2018, 9, 1) },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222221117"), FirstName = "Sofia", LastName = "Jiménez Moreno", Specialty = "Educación Infantil", HireDate = new DateTime(2020, 9, 1) },
            new() { Id = Guid.Parse("22222222-2222-2222-2222-222222221118"), FirstName = "Miguel", LastName = "Sánchez Castillo", Specialty = "Música", HireDate = new DateTime(2021, 9, 1) }
        };
        context.Teachers.AddRange(teachers);

        var classrooms = new List<Classroom>
        {
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331111"), GradeLevel = GradeLevel.Infantile3, Line = ClassroomLine.A, SchoolId = school.Id, TutorId = teachers[0].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331112"), GradeLevel = GradeLevel.Infantile3, Line = ClassroomLine.B, SchoolId = school.Id, TutorId = teachers[6].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331113"), GradeLevel = GradeLevel.Infantile4, Line = ClassroomLine.A, SchoolId = school.Id, TutorId = teachers[0].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331114"), GradeLevel = GradeLevel.Infantile4, Line = ClassroomLine.B, SchoolId = school.Id, TutorId = teachers[6].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331115"), GradeLevel = GradeLevel.Infantile5, Line = ClassroomLine.A, SchoolId = school.Id, TutorId = teachers[0].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331116"), GradeLevel = GradeLevel.Infantile5, Line = ClassroomLine.B, SchoolId = school.Id, TutorId = teachers[6].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331117"), GradeLevel = GradeLevel.Primary1, Line = ClassroomLine.A, SchoolId = school.Id, TutorId = teachers[1].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331118"), GradeLevel = GradeLevel.Primary1, Line = ClassroomLine.B, SchoolId = school.Id, TutorId = teachers[2].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331119"), GradeLevel = GradeLevel.Primary2, Line = ClassroomLine.A, SchoolId = school.Id, TutorId = teachers[1].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331120"), GradeLevel = GradeLevel.Primary2, Line = ClassroomLine.B, SchoolId = school.Id, TutorId = teachers[2].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331121"), GradeLevel = GradeLevel.Primary3, Line = ClassroomLine.A, SchoolId = school.Id, TutorId = teachers[3].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331122"), GradeLevel = GradeLevel.Primary3, Line = ClassroomLine.B, SchoolId = school.Id, TutorId = teachers[3].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331123"), GradeLevel = GradeLevel.Primary4, Line = ClassroomLine.A, SchoolId = school.Id, TutorId = teachers[4].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331124"), GradeLevel = GradeLevel.Primary4, Line = ClassroomLine.B, SchoolId = school.Id, TutorId = teachers[5].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331125"), GradeLevel = GradeLevel.Primary5, Line = ClassroomLine.A, SchoolId = school.Id, TutorId = teachers[4].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331126"), GradeLevel = GradeLevel.Primary5, Line = ClassroomLine.B, SchoolId = school.Id, TutorId = teachers[5].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331127"), GradeLevel = GradeLevel.Primary6, Line = ClassroomLine.A, SchoolId = school.Id, TutorId = teachers[7].Id },
            new() { Id = Guid.Parse("33333333-3333-3333-3333-333333331128"), GradeLevel = GradeLevel.Primary6, Line = ClassroomLine.B, SchoolId = school.Id, TutorId = teachers[7].Id }
        };
        context.Classrooms.AddRange(classrooms);

        var parents = new List<Parent>
        {
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444441111"), FirstName = "Antonio", LastName = "Fernández", Email = "antonio.fernandez@email.es", Phone = "612345678", Address = "Av. Roma 45, Madrid" },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444441112"), FirstName = "Carmen", LastName = "Díaz Sánchez", Email = "carmen.diaz@email.es", Phone = "612345679", Address = "Calle Valencia 12, Madrid" },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444441113"), FirstName = "Roberto", LastName = "Gómez Ruiz", Email = "roberto.gomez@email.es", Phone = "612345680", Address = "Plaza España 8, Madrid" },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444441114"), FirstName = " Isabel", LastName = "Martín Torres", Email = "isabel.martin@email.es", Phone = "612345681", Address = "Calle Mayor 67, Madrid" },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444441115"), FirstName = "Pablo", LastName = "Castro López", Email = "pablo.castro@email.es", Phone = "612345682", Address = "Av. Castilla 23, Madrid" }
        };
        context.Parents.AddRange(parents);

        var students = new List<Student>
        {
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555551111"), FirstName = "Alejandro", LastName = "Fernández García", DateOfBirth = new DateTime(2021, 3, 15), ClassroomId = classrooms[0].Id },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555551112"), FirstName = "María", LastName = "Fernández Díaz", DateOfBirth = new DateTime(2021, 5, 20), ClassroomId = classrooms[1].Id },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555551113"), FirstName = "Lucas", LastName = "Díaz Martínez", DateOfBirth = new DateTime(2020, 2, 10), ClassroomId = classrooms[2].Id },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555551114"), FirstName = "Sofia", LastName = "Gómez Sánchez", DateOfBirth = new DateTime(2020, 7, 8), ClassroomId = classrooms[3].Id },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555551115"), FirstName = "Mateo", LastName = "Ruiz Torres", DateOfBirth = new DateTime(2019, 11, 25), ClassroomId = classrooms[6].Id },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555551116"), FirstName = "Valentina", LastName = "Martín López", DateOfBirth = new DateTime(2019, 4, 3), ClassroomId = classrooms[7].Id },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555551117"), FirstName = "Diego", LastName = "Castro Fernández", DateOfBirth = new DateTime(2018, 9, 12), ClassroomId = classrooms[12].Id },
            new() { Id = Guid.Parse("55555555-5555-5555-5555-555555551118"), FirstName = "Emma", LastName = "Sánchez Gómez", DateOfBirth = new DateTime(2018, 1, 30), ClassroomId = classrooms[13].Id }
        };
        context.Students.AddRange(students);

        var studentParents = new List<StudentParent>
        {
            new() { StudentId = students[0].Id, ParentId = parents[0].Id, Relationship = RelationshipType.Father },
            new() { StudentId = students[0].Id, ParentId = parents[1].Id, Relationship = RelationshipType.Mother },
            new() { StudentId = students[1].Id, ParentId = parents[0].Id, Relationship = RelationshipType.Father },
            new() { StudentId = students[1].Id, ParentId = parents[1].Id, Relationship = RelationshipType.Mother },
            new() { StudentId = students[2].Id, ParentId = parents[1].Id, Relationship = RelationshipType.Mother },
            new() { StudentId = students[3].Id, ParentId = parents[2].Id, Relationship = RelationshipType.Father },
            new() { StudentId = students[4].Id, ParentId = parents[3].Id, Relationship = RelationshipType.Mother },
            new() { StudentId = students[5].Id, ParentId = parents[3].Id, Relationship = RelationshipType.Mother },
            new() { StudentId = students[5].Id, ParentId = parents[4].Id, Relationship = RelationshipType.Father },
            new() { StudentId = students[6].Id, ParentId = parents[4].Id, Relationship = RelationshipType.Father },
            new() { StudentId = students[7].Id, ParentId = parents[2].Id, Relationship = RelationshipType.Mother }
        };
        context.StudentParents.AddRange(studentParents);

        var invoices = new List<Invoice>
        {
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661111"), ParentId = parents[0].Id, StudentId = students[0].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 350.00m, Status = InvoiceStatus.Pending, Concept = InvoiceConcept.Monthly },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661112"), ParentId = parents[0].Id, StudentId = students[1].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 350.00m, Status = InvoiceStatus.Pending, Concept = InvoiceConcept.Monthly },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661113"), ParentId = parents[1].Id, StudentId = students[2].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 350.00m, Status = InvoiceStatus.Pending, Concept = InvoiceConcept.Monthly },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661114"), ParentId = parents[2].Id, StudentId = students[3].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 420.00m, Status = InvoiceStatus.Pending, Concept = InvoiceConcept.Monthly },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661115"), ParentId = parents[3].Id, StudentId = students[4].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 350.00m, Status = InvoiceStatus.Paid, Concept = InvoiceConcept.Monthly },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661116"), ParentId = parents[4].Id, StudentId = students[6].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 350.00m, Status = InvoiceStatus.Pending, Concept = InvoiceConcept.Monthly }
        };
        context.Invoices.AddRange(invoices);

        await context.SaveChangesAsync();
    }
}