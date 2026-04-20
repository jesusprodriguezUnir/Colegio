using Colegio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ColegioDbContext context, bool force = false)
    {
        if (!force && await context.Schools.AnyAsync()) return;

        if (force)
        {
            await ClearAllDataAsync(context);
        }

        var school = SeedSchools(context);
        var teachers = SeedTeachers(context);
        var classrooms = SeedClassrooms(context, school, teachers);
        var parents = SeedParents(context);
        var students = SeedStudents(context, classrooms);
        
        SeedRelationships(context, students, parents);
        SeedInvoices(context, items: (parents, students));
        SeedSchedules(context, classrooms, teachers);

        await context.SaveChangesAsync();
    }

    private static School SeedSchools(ColegioDbContext context)
    {
        var school = new School
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Colegio Público Virgen del Carmen",
            CIF = "Q2868006E",
            Address = "Calle Mayor 123",
            City = "Madrid",
            PostalCode = "28013",
            Province = "Madrid",
            ContactPhone = "912345678",
            ContactEmail = "colegio@virgendelcarmen.es"
        };
        context.Schools.Add(school);
        return school;
    }

    private static List<Teacher> SeedTeachers(ColegioDbContext context)
    {
        var firstNames = new[] { "María", "José", "Ana", "Carlos", "Laura", "David", "Sofia", "Miguel", "Lucía", "Javier", "Elena", "Pablo", "Carmen", "Daniel", "Paula", "Adrián", "Marta", "Alejandro", "Sara", "Álvaro", "Raquel", "Manuel", "Irene", "Francisco", "Beatriz", "Ignacio", "Silvia", "Rubén", "Cristina", "Jorge" };
        var lastNames = new[] { "García López", "Martínez Sánchez", "Rodríguez Torres", "López Fernández", "González Ruiz", "Pérez Gómez", "Jiménez Moreno", "Sánchez Castillo", "Díaz Muñoz", "Perales Ruiz", "Vázquez Ortiz", "Castro Rey", "Blanco Vega", "Navarro Soler", "Serrano Caballero", "Molina Gil", "Morales Santos", "Ortega Delgado", "Delgado Flor", "Castillo Sol", "Medina Vera", "Vega Luna", "León Soto", "Herrero Rico", "Gil Marín", "Vicente Paz", "Sanz Cruz", "Fuentes Lobo", "Cruz Solis", "Ibáñez Mar" };
        var specialties = new[] { "Educación Infantil", "Matemáticas", "Lengua", "Ciencias", "Educación Física", "Inglés", "Música", "Artes", "Geografía e Historia", "Física y Química", "Apoyo Educativo", "Psicología" };

        var teachers = new List<Teacher>();
        var random = new Random(42);

        for (int i = 0; i < 30; i++)
        {
            var firstName = firstNames[i % firstNames.Length];
            var lastName = lastNames[i % lastNames.Length];
            var specialty = specialties[random.Next(specialties.Length)];
            
            teachers.Add(new Teacher
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Specialty = specialty,
                Email = $"{firstName.ToLower().Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")}.{lastName.Split(' ')[0].ToLower()}@colegiocarmen.es",
                Phone = $"6{random.Next(10000000, 99999999)}",
                IBAN = $"ES{random.Next(10, 99)} 2100 {random.Next(1000, 9999)} {random.Next(10, 99)} {random.Next(10000000, 99999999)}",
                DateOfBirth = new DateTime(random.Next(1970, 1995), random.Next(1, 13), random.Next(1, 28)),
                HireDate = new DateTime(random.Next(2010, 2023), 9, 1)
            });
        }

        context.Teachers.AddRange(teachers);
        return teachers;
    }

    private static List<Classroom> SeedClassrooms(ColegioDbContext context, School school, List<Teacher> teachers)
    {
        var classrooms = new List<Classroom>();
        var grades = Enum.GetValues<GradeLevel>();
        var lines = Enum.GetValues<ClassroomLine>();
        int teacherIndex = 0;

        foreach (var grade in grades)
        {
            foreach (var line in lines)
            {
                classrooms.Add(new Classroom
                {
                    Id = Guid.NewGuid(),
                    GradeLevel = grade,
                    Line = line,
                    SchoolId = school.Id,
                    TutorId = teacherIndex < teachers.Count ? teachers[teacherIndex++].Id : null
                });
            }
        }

        context.Classrooms.AddRange(classrooms);
        return classrooms;
    }

    private static List<Parent> SeedParents(ColegioDbContext context)
    {
        var parents = new List<Parent>
        {
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444441111"), FirstName = "Antonio", LastName = "Fernández", Email = "antonio.fernandez@email.es", Phone = "612345678", Address = "Av. Roma 45, Madrid" },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444441112"), FirstName = "Carmen", LastName = "Díaz Sánchez", Email = "carmen.diaz@email.es", Phone = "612345679", Address = "Calle Valencia 12, Madrid" },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444441113"), FirstName = "Roberto", LastName = "Gómez Ruiz", Email = "roberto.gomez@email.es", Phone = "612345680", Address = "Plaza España 8, Madrid" },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444441114"), FirstName = " Isabel", LastName = "Martín Torres", Email = "isabel.martin@email.es", Phone = "612345681", Address = "Calle Mayor 67, Madrid" },
            new() { Id = Guid.Parse("44444444-4444-4444-4444-444444441115"), FirstName = "Pablo", LastName = "Castro López", Email = "pablo.castro@email.es", Phone = "612345682", Address = "Av. Castilla 23, Madrid" }
        };
        context.Parents.AddRange(parents);
        return parents;
    }

    private static List<Student> SeedStudents(ColegioDbContext context, List<Classroom> classrooms)
    {
        var firstNames = new[] { "Alejandro", "María", "Lucas", "Sofia", "Mateo", "Valentina", "Diego", "Emma", "Daniel", "Lucía", "Hugo", "Martina", "Leo", "Paula", "Martín", "Julia", "Adrian", "Alba", "Pablo", "Elena", "Álvaro", "Sara", "Oliver", "Noa", "Thiago", "Carla", "Marc", "Irene", "Bruno", "Lola" };
        var lastNames = new[] { "García", "Rodríguez", "Martínez", "López", "González", "Pérez", "Sánchez", "Gómez", "Jiménez", "Díaz", "Ruiz", "Torres", "Vázquez", "Castro", "Romero", "Navarro", "Serrano", "Morales", "Ortega", "Delgado" };
        
        var students = new List<Student>();
        var random = new Random(42);

        foreach (var classroom in classrooms)
        {
            for (int i = 0; i < 20; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName1 = lastNames[random.Next(lastNames.Length)];
                var lastName2 = lastNames[random.Next(lastNames.Length)];
                
                students.Add(new Student
                {
                    Id = Guid.NewGuid(),
                    FirstName = firstName,
                    LastName = $"{lastName1} {lastName2}",
                    DateOfBirth = GetRandomBirthDate(classroom.GradeLevel, random),
                    ClassroomId = classroom.Id
                });
            }
        }
        
        context.Students.AddRange(students);
        return students;
    }

    private static DateTime GetRandomBirthDate(GradeLevel grade, Random random)
    {
        // 2026 is current year
        int baseAge = (int)grade + 2; 
        int birthYear = 2026 - baseAge;
        return new DateTime(birthYear, random.Next(1, 13), random.Next(1, 28));
    }

    private static void SeedRelationships(ColegioDbContext context, List<Student> students, List<Parent> parents)
    {
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
    }

    private static void SeedInvoices(ColegioDbContext context, (List<Parent> parents, List<Student> students) items)
    {
        var invoices = new List<Invoice>
        {
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661111"), ParentId = items.parents[0].Id, StudentId = items.students[0].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 350.00m, Status = InvoiceStatus.Pending, Concept = InvoiceConcept.Monthly },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661112"), ParentId = items.parents[0].Id, StudentId = items.students[1].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 350.00m, Status = InvoiceStatus.Pending, Concept = InvoiceConcept.Monthly },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661113"), ParentId = items.parents[1].Id, StudentId = items.students[2].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 350.00m, Status = InvoiceStatus.Pending, Concept = InvoiceConcept.Monthly },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661114"), ParentId = items.parents[2].Id, StudentId = items.students[3].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 420.00m, Status = InvoiceStatus.Pending, Concept = InvoiceConcept.Monthly },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661115"), ParentId = items.parents[3].Id, StudentId = items.students[4].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 350.00m, Status = InvoiceStatus.Paid, Concept = InvoiceConcept.Monthly },
            new() { Id = Guid.Parse("66666666-6666-6666-6666-666666661116"), ParentId = items.parents[4].Id, StudentId = items.students[6].Id, IssueDate = new DateTime(2026, 4, 1), DueDate = new DateTime(2026, 4, 30), TotalAmount = 350.00m, Status = InvoiceStatus.Pending, Concept = InvoiceConcept.Monthly }
        };
        context.Invoices.AddRange(invoices);
    }

    private static void SeedSchedules(ColegioDbContext context, List<Classroom> classrooms, List<Teacher> teachers)
    {
        var schedules = new List<Schedule>
        {
            new() { Id = Guid.NewGuid(), ClassroomId = classrooms[6].Id, TeacherId = teachers[1].Id, Subject = "Matemáticas", DayOfWeek = Domain.Entities.DayOfWeek.Monday, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 0, 0) },
            new() { Id = Guid.NewGuid(), ClassroomId = classrooms[6].Id, TeacherId = teachers[2].Id, Subject = "Lengua", DayOfWeek = Domain.Entities.DayOfWeek.Monday, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(11, 0, 0) },
            new() { Id = Guid.NewGuid(), ClassroomId = classrooms[7].Id, TeacherId = teachers[3].Id, Subject = "Ciencias", DayOfWeek = Domain.Entities.DayOfWeek.Tuesday, StartTime = new TimeSpan(11, 30, 0), EndTime = new TimeSpan(12, 30, 0) }
        };
        context.Schedules.AddRange(schedules);
    }

    public static async Task ClearAllDataAsync(ColegioDbContext context)
    {
        // El orden es importante para evitar violaciones de FK
        context.Invoices.RemoveRange(context.Invoices);
        context.StudentParents.RemoveRange(context.StudentParents);
        context.Schedules.RemoveRange(context.Schedules);
        context.Students.RemoveRange(context.Students);
        context.Classrooms.RemoveRange(context.Classrooms);
        context.Teachers.RemoveRange(context.Teachers);
        context.Parents.RemoveRange(context.Parents);
        context.Schools.RemoveRange(context.Schools);

        await context.SaveChangesAsync();
    }

    public static async Task<object> GetDatabaseStatsAsync(ColegioDbContext context)
    {
        return new
        {
            Schools = await context.Schools.CountAsync(),
            Teachers = await context.Teachers.CountAsync(),
            Classrooms = await context.Classrooms.CountAsync(),
            Students = await context.Students.CountAsync(),
            Parents = await context.Parents.CountAsync(),
            Schedules = await context.Schedules.CountAsync(),
            Invoices = await context.Invoices.CountAsync()
        };
    }
}