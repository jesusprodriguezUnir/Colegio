using Colegio.Domain.Entities;
using Colegio.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ColegioDbContext context, IScheduleGenerator? generator = null, bool force = false)
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
        var rooms = SeedRooms(context);
        SeedCurriculum(context, subjects);
        
        var timeSlots = SeedTimeSlots(context);
        SeedTeacherCompetencies(context, teachers, subjects);
        SeedTeacherAvailability(context, teachers, timeSlots);
        SeedConstraints(context);
        
        SeedClassUnits(context, classrooms, subjects, teachers);
        
        // We MUST save here so the generator can find the data in the DB
        await context.SaveChangesAsync();

        if (generator != null)
        {
            // Generate for Standard session
            await generator.GenerateAllAsync(AcademicSessionType.Standard);
            // Generate for Intensive session
            await generator.GenerateAllAsync(AcademicSessionType.Intensive);
        }
        else
        {
            // Fallback to minimal hardcoded seeding if no generator provided
            SeedSchedules(context, classrooms, teachers, subjects, timeSlots);
            await context.SaveChangesAsync();
        }
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

        for (int i = 0; i < 45; i++)
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
                Email = $"{firstName.ToLower().Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")}.{lastName.Split(' ')[0].ToLower()}{i}@colegiocarmen.es",
                Phone = $"6{random.Next(10000000, 99999999)}",
                IBAN = $"ES{random.Next(10, 99)} 2100 {random.Next(1000, 9999)} {random.Next(10, 99)} {random.Next(10000000, 99999999)}",
                DateOfBirth = new DateTime(random.Next(1970, 1995), random.Next(1, 13), random.Next(1, 28)),
                HireDate = new DateTime(random.Next(2010, 2023), 9, 1),
                MaxWorkingHours = random.Next(20, 35)
            });
        }

        context.Teachers.AddRange(teachers);
        return teachers;
    }

    private static List<Classroom> SeedClassrooms(ColegioDbContext context, School school, List<Teacher> teachers)
    {
        var classrooms = new List<Classroom>();
        var grades = new[] { GradeLevel.Primary1, GradeLevel.Primary2, GradeLevel.Primary3,
                             GradeLevel.Primary4, GradeLevel.Primary5, GradeLevel.Primary6 };
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
        int i = 0;
        foreach (var teacher in teachers)
        {
            i++;
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
            if (teacher.Specialty == "Geografía e Historia") teacher.Subjects.Add(subjects["Geografía e Historia"]);
            if (teacher.Specialty == "Física y Química") 
            {
                teacher.Subjects.Add(subjects["Física y Química"]);
                teacher.Subjects.Add(subjects["Tecnología y Digitalización"]);
            }
            if (teacher.Specialty == "Apoyo Educativo" || teacher.Specialty == "Psicología") 
            {
                teacher.Subjects.Add(subjects["Ed. en Valores Cívicos y Éticos"]);
                teacher.Subjects.Add(subjects["Atención Educativa"]);
                teacher.Subjects.Add(subjects["Materia Optativa"]);
            }
            
            // Infantil/Primaria can teach almost anything basic
            if (teacher.Specialty == "Educación Infantil")
            {
                teacher.Subjects.Add(subjects["Tutoría"]);
                teacher.Subjects.Add(subjects["Religión"]);
                teacher.Subjects.Add(subjects["Lengua Castellana y Literatura"]);
                teacher.Subjects.Add(subjects["Matemáticas"]);
                teacher.Subjects.Add(subjects["Música"]);
                teacher.Subjects.Add(subjects["Biología y Geología"]);
            }
            
            // Ensure Filosofía subjects have someone
            if (i % 5 == 0) 
            {
                teacher.Subjects.Add(subjects["Filosofía"]);
                teacher.Subjects.Add(subjects["Historia de la Filosofía"]);
                teacher.Subjects.Add(subjects["Historia de España"]);
            }
            
            // Add some generic subjects to many teachers to avoid bottlenecks
            if (i % 3 == 0)
            {
                teacher.Subjects.Add(subjects["Tutoría"]);
                teacher.Subjects.Add(subjects["Atención Educativa"]);
                teacher.Subjects.Add(subjects["Materia Optativa"]);
            }
        }
    }
    private static List<Room> SeedRooms(ColegioDbContext context)
    {
        var rooms = new List<Room>
        {
            new() { Id = Guid.NewGuid(), Name = "Laboratorio de Ciencias", Type = RoomType.Specific, Capacity = 30, Building = "Pabellón A", Floor = 1 },
            new() { Id = Guid.NewGuid(), Name = "Gimnasio Polideportivo", Type = RoomType.Specific, Capacity = 60, Building = "Pabellón Deportivo", Floor = 0 },
            new() { Id = Guid.NewGuid(), Name = "Aula de Música", Type = RoomType.Specific, Capacity = 25, Building = "Pabellón B", Floor = 2 },
            new() { Id = Guid.NewGuid(), Name = "Aula de Informática", Type = RoomType.Specific, Capacity = 25, Building = "Pabellón B", Floor = 2 },
            new() { Id = Guid.NewGuid(), Name = "Taller de Tecnología", Type = RoomType.Specific, Capacity = 20, Building = "Pabellón A", Floor = 0 }
        };
        context.Rooms.AddRange(rooms);
        return rooms;
    }

    private static void SeedConstraints(ColegioDbContext context)
    {
        var constraints = new List<ScheduleConstraint>
        {
            new() { Id = Guid.NewGuid(), Type = ConstraintType.MaxDailyHours, Hardness = ConstraintHardness.Hard, Parameters = "{\"MaxHours\": 6}", IsActive = true, Description = "Máximo 6 horas lectivas diarias por profesor" },
            new() { Id = Guid.NewGuid(), Type = ConstraintType.CompactSchedule, Hardness = ConstraintHardness.Soft, Weight = 8, IsActive = true, Description = "Intentar agrupar las horas del profesor sin huecos" },
            new() { Id = Guid.NewGuid(), Type = ConstraintType.NonConsecutiveDays, Hardness = ConstraintHardness.Soft, Weight = 5, IsActive = true, Description = "Evitar misma asignatura en días consecutivos si es posible" }
        };
        context.ScheduleConstraints.AddRange(constraints);
    }

    private static void SeedTeacherAvailability(ColegioDbContext context, List<Teacher> teachers, List<TimeSlot> timeSlots)
    {
        var availabilities = new List<TeacherAvailability>();
        var random = new Random(42);
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
                    IsAvailable = true,
                    Level = random.Next(10) > 8 ? AvailabilityLevel.Preferred : AvailabilityLevel.Available
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
        var subjectData = new[]
        {
            ("Lengua Castellana y Literatura", "#ef4444", (string?)null),
            ("Matemáticas", "#3b82f6", (string?)null),
            ("Geografía e Historia", "#f59e0b", (string?)null),
            ("Lengua Extranjera (Inglés)", "#10b981", (string?)null),
            ("Educación Física", "#ec4899", "Gimnasio Polideportivo"),
            ("Biología y Geología", "#8b5cf6", "Laboratorio de Ciencias"),
            ("Física y Química", "#6366f1", "Laboratorio de Ciencias"),
            ("Tecnología y Digitalización", "#14b8a6", "Taller de Tecnología"),
            ("Ed. Plástica, Visual y Audiovisual", "#f97316", (string?)null),
            ("Música", "#06b6d4", "Aula de Música"),
            ("Ed. en Valores Cívicos y Éticos", "#84cc16", (string?)null),
            ("Religión", "#94a3b8", (string?)null),
            ("Atención Educativa", "#94a3b8", (string?)null),
            ("Materia Optativa", "#0ea5e9", (string?)null),
            ("Tutoría", "#64748b", (string?)null),
            ("Filosofía", "#d946ef", (string?)null),
            ("Historia de la Filosofía", "#d946ef", (string?)null),
            ("Historia de España", "#f43f5e", (string?)null)
        };

        var rooms = context.Rooms.Local.ToList();
        var subjects = subjectData.Select(s => new Subject 
        { 
            Id = Guid.NewGuid(), 
            Name = s.Item1, 
            Color = s.Item2,
            RequiredRoomId = s.Item3 != null ? rooms.FirstOrDefault(r => r.Name == s.Item3)?.Id : null
        }).ToDictionary(s => s.Name);

        context.Subjects.AddRange(subjects.Values);
        return subjects;
    }

    private static void SeedCurriculum(ColegioDbContext context, Dictionary<string, Subject> subjects)
    {
        var curriculum = new List<Curriculum>();

        // Primary 1
        AddEntry(curriculum, GradeLevel.Primary1, subjects["Lengua Castellana y Literatura"], 7);
        AddEntry(curriculum, GradeLevel.Primary1, subjects["Matemáticas"], 5);
        AddEntry(curriculum, GradeLevel.Primary1, subjects["Biología y Geología"], 3);
        AddEntry(curriculum, GradeLevel.Primary1, subjects["Educación Física"], 3);
        AddEntry(curriculum, GradeLevel.Primary1, subjects["Música"], 2);
        AddEntry(curriculum, GradeLevel.Primary1, subjects["Religión"], 2);
        AddEntry(curriculum, GradeLevel.Primary1, subjects["Tutoría"], 1);

        // Primary 2
        AddEntry(curriculum, GradeLevel.Primary2, subjects["Lengua Castellana y Literatura"], 7);
        AddEntry(curriculum, GradeLevel.Primary2, subjects["Matemáticas"], 5);
        AddEntry(curriculum, GradeLevel.Primary2, subjects["Biología y Geología"], 3);
        AddEntry(curriculum, GradeLevel.Primary2, subjects["Educación Física"], 3);
        AddEntry(curriculum, GradeLevel.Primary2, subjects["Música"], 2);
        AddEntry(curriculum, GradeLevel.Primary2, subjects["Religión"], 2);
        AddEntry(curriculum, GradeLevel.Primary2, subjects["Tutoría"], 1);

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

        context.Curriculums.AddRange(curriculum);
    }

    private static void AddEntry(List<Curriculum> list, GradeLevel grade, Subject subject, int hours)
    {
        list.Add(new Curriculum { Id = Guid.NewGuid(), GradeLevel = grade, SubjectId = subject.Id, WeeklyHours = hours });
    }

    private static void SeedClassUnits(ColegioDbContext context, List<Classroom> classrooms, Dictionary<string, Subject> subjects, List<Teacher> teachers)
    {
        var classUnits = new List<ClassUnit>();
        var curriculums = context.Curriculums.Local.ToList();
        var random = new Random(42);

        // Only generate for Primary 1–6
        var eligibleClassrooms = classrooms
            .Where(c => c.GradeLevel >= GradeLevel.Primary1)
            .ToList();

        foreach (var classroom in eligibleClassrooms)
        {
            var gradeCurriculum = curriculums
                .Where(c => c.GradeLevel == classroom.GradeLevel)
                .ToList();

            foreach (var entry in gradeCurriculum)
            {
                var subject = subjects.Values.FirstOrDefault(s => s.Id == entry.SubjectId);
                if (subject == null) continue;

                // Find a competent teacher for this subject
                var competentTeacher = teachers
                    .Where(t => t.Subjects.Any(s => s.Id == subject.Id))
                    .OrderBy(_ => random.Next())
                    .FirstOrDefault();

                classUnits.Add(new ClassUnit
                {
                    Id = Guid.NewGuid(),
                    ClassroomId = classroom.Id,
                    SubjectId = subject.Id,
                    TeacherId = competentTeacher?.Id,
                    WeeklySessions = entry.WeeklyHours,
                    SessionDuration = 1,
                    AllowConsecutiveDays = true,
                    PreferNonConsecutive = false, // Relaxed
                    AllowDoubleSession = true, // Relaxed
                    MaxSessionsPerDay = entry.WeeklyHours >= 8 ? 3 : (entry.WeeklyHours >= 4 ? 2 : 1),
                    PreferredRoomId = subject.RequiredRoomId,
                    IsActive = true
                });
            }
        }

        context.ClassUnits.AddRange(classUnits);
    }

    public static async Task ClearAllDataAsync(ColegioDbContext context)
    {
        // El orden es importante para evitar violaciones de FK
        context.Invoices.RemoveRange(context.Invoices);
        context.StudentParents.RemoveRange(context.StudentParents);
        context.Schedules.RemoveRange(context.Schedules);
        context.ClassUnits.RemoveRange(context.ClassUnits);
        context.Students.RemoveRange(context.Students);
        context.Classrooms.RemoveRange(context.Classrooms);
        context.Teachers.RemoveRange(context.Teachers);
        context.Parents.RemoveRange(context.Parents);
        context.Curriculums.RemoveRange(context.Curriculums);
        context.Subjects.RemoveRange(context.Subjects);
        context.TimeSlots.RemoveRange(context.TimeSlots);
        context.TeacherAvailabilities.RemoveRange(context.TeacherAvailabilities);
        context.ScheduleConstraints.RemoveRange(context.ScheduleConstraints);
        context.Rooms.RemoveRange(context.Rooms);
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
            CurriculumEntries = await context.Curriculums.CountAsync(),
            ClassUnits = await context.ClassUnits.CountAsync()
        };
    }
}