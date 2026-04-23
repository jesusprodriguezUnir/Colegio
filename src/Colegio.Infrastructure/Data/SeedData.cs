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
        
        var subjects = SeedSubjects(context);
        SeedCurriculum(context, subjects);
        
        var timeSlots = SeedTimeSlots(context);
        SeedTeacherCompetencies(context, teachers, subjects);
        SeedTeacherAvailability(context, teachers, timeSlots);
        
        SeedSchedules(context, classrooms, teachers, subjects, timeSlots);

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
                HireDate = new DateTime(random.Next(2010, 2023), 9, 1),
                MaxWorkingHours = random.Next(15, 30)
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
    private static List<TimeSlot> SeedTimeSlots(ColegioDbContext context)
    {
        var timeSlots = new List<TimeSlot>();
        var days = Enum.GetValues<Domain.Entities.DayOfWeek>();

        foreach (var day in days)
        {
            // Standard Session (Oct-May)
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = day, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 0, 0), Label = "1ª Hora" });
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = day, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(11, 0, 0), Label = "2ª Hora" });
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = day, StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(12, 0, 0), Label = "3ª Hora" });
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = day, StartTime = new TimeSpan(12, 0, 0), EndTime = new TimeSpan(13, 0, 0), Label = "4ª Hora" });
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = day, StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(15, 0, 0), IsBreak = true, Label = "Comida" });
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = day, StartTime = new TimeSpan(15, 0, 0), EndTime = new TimeSpan(16, 0, 0), Label = "5ª Hora" });
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Standard, DayOfWeek = day, StartTime = new TimeSpan(16, 0, 0), EndTime = new TimeSpan(17, 0, 0), Label = "6ª Hora" });

            // Intensive Session (Jun/Sep)
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Intensive, DayOfWeek = day, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(10, 0, 0), Label = "1ª Hora" });
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Intensive, DayOfWeek = day, StartTime = new TimeSpan(10, 0, 0), EndTime = new TimeSpan(11, 0, 0), Label = "2ª Hora" });
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Intensive, DayOfWeek = day, StartTime = new TimeSpan(11, 0, 0), EndTime = new TimeSpan(12, 0, 0), Label = "3ª Hora" });
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Intensive, DayOfWeek = day, StartTime = new TimeSpan(12, 0, 0), EndTime = new TimeSpan(13, 0, 0), Label = "4ª Hora" });
            timeSlots.Add(new TimeSlot { Id = Guid.NewGuid(), SessionType = AcademicSessionType.Intensive, DayOfWeek = day, StartTime = new TimeSpan(13, 0, 0), EndTime = new TimeSpan(14, 0, 0), Label = "5ª Hora" });
        }

        context.TimeSlots.AddRange(timeSlots);
        return timeSlots;
    }

    private static void SeedTeacherCompetencies(ColegioDbContext context, List<Teacher> teachers, Dictionary<string, Subject> subjects)
    {
        foreach (var teacher in teachers)
        {
            // Map specialty to subjects
            if (teacher.Specialty == "Matemáticas") teacher.Subjects.Add(subjects["Matemáticas"]);
            if (teacher.Specialty == "Lengua") teacher.Subjects.Add(subjects["Lengua Castellana y Literatura"]);
            if (teacher.Specialty == "Ciencias") 
            {
                teacher.Subjects.Add(subjects["Biología y Geología"]);
                teacher.Subjects.Add(subjects["Física y Química"]);
            }
            if (teacher.Specialty == "Educación Física") teacher.Subjects.Add(subjects["Educación Física"]);
            if (teacher.Specialty == "Inglés") teacher.Subjects.Add(subjects["Lengua Extranjera (Inglés)"]);
            if (teacher.Specialty == "Música") teacher.Subjects.Add(subjects["Música"]);
            if (teacher.Specialty == "Artes") teacher.Subjects.Add(subjects["Ed. Plástica, Visual y Audiovisual"]);
            
            // Infantil/Primaria can teach almost anything basic
            if (teacher.Specialty == "Educación Infantil")
            {
                teacher.Subjects.Add(subjects["Tutoría"]);
                teacher.Subjects.Add(subjects["Religión"]);
            }
        }
    }

    private static void SeedTeacherAvailability(ColegioDbContext context, List<Teacher> teachers, List<TimeSlot> timeSlots)
    {
        var availabilities = new List<TeacherAvailability>();
        foreach (var teacher in teachers)
        {
            foreach (var slot in timeSlots)
            {
                if (slot.IsBreak) continue;
                
                availabilities.Add(new TeacherAvailability
                {
                    Id = Guid.NewGuid(),
                    TeacherId = teacher.Id,
                    TimeSlotId = slot.Id,
                    IsAvailable = true
                });
            }
        }
        context.TeacherAvailabilities.AddRange(availabilities);
    }

    private static void SeedSchedules(ColegioDbContext context, List<Classroom> classrooms, List<Teacher> teachers, Dictionary<string, Subject> subjects, List<TimeSlot> timeSlots)
    {
        var math = subjects["Matemáticas"];
        var language = subjects["Lengua Castellana y Literatura"];
        
        var monday1 = timeSlots.First(ts => ts.DayOfWeek == Domain.Entities.DayOfWeek.Monday && ts.StartTime == new TimeSpan(9, 0, 0) && ts.SessionType == AcademicSessionType.Standard);
        var monday2 = timeSlots.First(ts => ts.DayOfWeek == Domain.Entities.DayOfWeek.Monday && ts.StartTime == new TimeSpan(10, 0, 0) && ts.SessionType == AcademicSessionType.Standard);

        var schedules = new List<Schedule>
        {
            new() { Id = Guid.NewGuid(), ClassroomId = classrooms[6].Id, TeacherId = teachers.First(t => t.Subjects.Contains(math)).Id, SubjectId = math.Id, TimeSlotId = monday1.Id },
            new() { Id = Guid.NewGuid(), ClassroomId = classrooms[6].Id, TeacherId = teachers.First(t => t.Subjects.Contains(language)).Id, SubjectId = language.Id, TimeSlotId = monday2.Id }
        };
        context.Schedules.AddRange(schedules);
    }

    private static Dictionary<string, Subject> SeedSubjects(ColegioDbContext context)
    {
        var subjectNames = new[]
        {
            "Lengua Castellana y Literatura", "Matemáticas", "Geografía e Historia", "Lengua Extranjera (Inglés)",
            "Educación Física", "Biología y Geología", "Física y Química", "Tecnología y Digitalización",
            "Ed. Plástica, Visual y Audiovisual", "Música", "Ed. en Valores Cívicos y Éticos", "Religión",
            "Atención Educativa", "Materia Optativa", "Tutoría", "Filosofía", "Historia de la Filosofía", "Historia de España"
        };

        var subjects = subjectNames.Select(name => new Subject { Id = Guid.NewGuid(), Name = name }).ToDictionary(s => s.Name);
        context.Subjects.AddRange(subjects.Values);
        return subjects;
    }

    private static void SeedCurriculum(ColegioDbContext context, Dictionary<string, Subject> subjects)
    {
        var curriculum = new List<Curriculum>();

        // Primary 3
        AddEntry(curriculum, GradeLevel.Primary3, subjects["Lengua Castellana y Literatura"], 8);
        AddEntry(curriculum, GradeLevel.Primary3, subjects["Matemáticas"], 6);
        AddEntry(curriculum, GradeLevel.Primary3, subjects["Educación Física"], 3);
        AddEntry(curriculum, GradeLevel.Primary3, subjects["Tutoría"], 1);

        // Primary 4
        AddEntry(curriculum, GradeLevel.Primary4, subjects["Lengua Castellana y Literatura"], 8);
        AddEntry(curriculum, GradeLevel.Primary4, subjects["Matemáticas"], 6);
        AddEntry(curriculum, GradeLevel.Primary4, subjects["Educación Física"], 3);
        AddEntry(curriculum, GradeLevel.Primary4, subjects["Tutoría"], 1);

        // Primary 5
        AddEntry(curriculum, GradeLevel.Primary5, subjects["Lengua Castellana y Literatura"], 8);
        AddEntry(curriculum, GradeLevel.Primary5, subjects["Matemáticas"], 6);
        AddEntry(curriculum, GradeLevel.Primary5, subjects["Educación Física"], 3);
        AddEntry(curriculum, GradeLevel.Primary5, subjects["Tutoría"], 1);

        // Primary 6
        AddEntry(curriculum, GradeLevel.Primary6, subjects["Lengua Castellana y Literatura"], 8);
        AddEntry(curriculum, GradeLevel.Primary6, subjects["Matemáticas"], 6);
        AddEntry(curriculum, GradeLevel.Primary6, subjects["Educación Física"], 3);
        AddEntry(curriculum, GradeLevel.Primary6, subjects["Tutoría"], 1);

        // ESO 1
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Lengua Castellana y Literatura"], 5);
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Matemáticas"], 4);
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Lengua Extranjera (Inglés)"], 3);
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Geografía e Historia"], 3);
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Educación Física"], 3);
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Biología y Geología"], 3);
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Ed. Plástica, Visual y Audiovisual"], 2);
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Música"], 2);
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Religión"], 2);
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Materia Optativa"], 2);
        AddEntry(curriculum, GradeLevel.ESO1, subjects["Tutoría"], 1);

        // ESO 2
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Lengua Castellana y Literatura"], 4);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Matemáticas"], 4);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Lengua Extranjera (Inglés)"], 3);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Geografía e Historia"], 3);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Educación Física"], 3);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Física y Química"], 3);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Tecnología y Digitalización"], 3);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Ed. Plástica, Visual y Audiovisual"], 2);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Ed. en Valores Cívicos y Éticos"], 1);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Religión"], 1);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Materia Optativa"], 2);
        AddEntry(curriculum, GradeLevel.ESO2, subjects["Tutoría"], 1);

        // ESO 3
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Lengua Castellana y Literatura"], 4);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Matemáticas"], 4);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Lengua Extranjera (Inglés)"], 3);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Geografía e Historia"], 3);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Educación Física"], 3);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Biología y Geología"], 2);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Física y Química"], 3);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Tecnología y Digitalización"], 2);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Música"], 2);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Religión"], 1);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Materia Optativa"], 2);
        AddEntry(curriculum, GradeLevel.ESO3, subjects["Tutoría"], 1);

        // ESO 4
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Lengua Castellana y Literatura"], 4);
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Matemáticas"], 4);
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Lengua Extranjera (Inglés)"], 3);
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Geografía e Historia"], 3);
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Educación Física"], 2);
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Religión"], 2);
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Materia Optativa"], 2);
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Tutoría"], 1);
        // Materias de opción (ejemplos)
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Biología y Geología"], 3);
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Física y Química"], 3);
        AddEntry(curriculum, GradeLevel.ESO4, subjects["Tecnología y Digitalización"], 3);

        // Bachillerato 1
        AddEntry(curriculum, GradeLevel.Bachillerato1, subjects["Lengua Castellana y Literatura"], 4);
        AddEntry(curriculum, GradeLevel.Bachillerato1, subjects["Lengua Extranjera (Inglés)"], 3);
        AddEntry(curriculum, GradeLevel.Bachillerato1, subjects["Filosofía"], 3);
        AddEntry(curriculum, GradeLevel.Bachillerato1, subjects["Educación Física"], 2);
        AddEntry(curriculum, GradeLevel.Bachillerato1, subjects["Materia Optativa"], 3);
        AddEntry(curriculum, GradeLevel.Bachillerato1, subjects["Religión"], 1);
        AddEntry(curriculum, GradeLevel.Bachillerato1, subjects["Tutoría"], 1);
        // Modalidad (ejemplos)
        AddEntry(curriculum, GradeLevel.Bachillerato1, subjects["Matemáticas"], 4);

        // Bachillerato 2
        AddEntry(curriculum, GradeLevel.Bachillerato2, subjects["Lengua Castellana y Literatura"], 4);
        AddEntry(curriculum, GradeLevel.Bachillerato2, subjects["Lengua Extranjera (Inglés)"], 3);
        AddEntry(curriculum, GradeLevel.Bachillerato2, subjects["Historia de España"], 3);
        AddEntry(curriculum, GradeLevel.Bachillerato2, subjects["Historia de la Filosofía"], 3);
        AddEntry(curriculum, GradeLevel.Bachillerato2, subjects["Materia Optativa"], 3);
        AddEntry(curriculum, GradeLevel.Bachillerato2, subjects["Religión"], 1);
        AddEntry(curriculum, GradeLevel.Bachillerato2, subjects["Tutoría"], 1);
        // Modalidad (ejemplos)
        AddEntry(curriculum, GradeLevel.Bachillerato2, subjects["Matemáticas"], 4);

        context.Curriculums.AddRange(curriculum);
    }

    private static void AddEntry(List<Curriculum> list, GradeLevel grade, Subject subject, int hours)
    {
        list.Add(new Curriculum { Id = Guid.NewGuid(), GradeLevel = grade, SubjectId = subject.Id, WeeklyHours = hours });
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
        context.Curriculums.RemoveRange(context.Curriculums);
        context.Subjects.RemoveRange(context.Subjects);
        context.TimeSlots.RemoveRange(context.TimeSlots);
        context.TeacherAvailabilities.RemoveRange(context.TeacherAvailabilities);
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
            Invoices = await context.Invoices.CountAsync(),
            Subjects = await context.Subjects.CountAsync(),
            CurriculumEntries = await context.Curriculums.CountAsync()
        };
    }
}