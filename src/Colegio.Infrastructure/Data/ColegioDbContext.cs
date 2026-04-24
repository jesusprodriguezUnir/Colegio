using Colegio.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Infrastructure.Data;

public class ColegioDbContext : DbContext
{
    public ColegioDbContext(DbContextOptions<ColegioDbContext> options)
        : base(options)
    {
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    public DbSet<School> Schools => Set<School>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<StudentParent> StudentParents => Set<StudentParent>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Curriculum> Curriculums => Set<Curriculum>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<TeacherAvailability> TeacherAvailabilities => Set<TeacherAvailability>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<ScheduleConstraint> ScheduleConstraints => Set<ScheduleConstraint>();
    public DbSet<ClassUnit> ClassUnits => Set<ClassUnit>();
    public DbSet<TimetableFramework> TimetableFrameworks => Set<TimetableFramework>();
    public DbSet<BreakDefinition> BreakDefinitions => Set<BreakDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.ContactPhone).HasMaxLength(50);
            entity.Property(e => e.ContactEmail).HasMaxLength(100);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Specialty).HasMaxLength(200);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.IBAN).HasMaxLength(50);
            entity.Property(e => e.MaxWorkingHours).IsRequired().HasDefaultValue(25);
            entity.Property(e => e.MaxGapsPerDay).HasDefaultValue(1);
            entity.Property(e => e.MinDailyHours).HasDefaultValue(2);
            entity.Property(e => e.PreferCompactSchedule).HasDefaultValue(true);
        });

        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.School)
                .WithMany(s => s.Classrooms)
                .HasForeignKey(e => e.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Tutor)
                .WithMany(t => t.TutoredClassrooms)
                .HasForeignKey(e => e.TutorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.HasOne(e => e.Classroom)
                .WithMany(c => c.Students)
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Parent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
        });

        modelBuilder.Entity<StudentParent>(entity =>
        {
            entity.HasKey(e => new { e.StudentId, e.ParentId });

            entity.HasOne(e => e.Student)
                .WithMany(s => s.StudentParents)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Parent)
                .WithMany(p => p.StudentParents)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Schedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Classroom)
                .WithMany(c => c.Schedules)
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Teacher)
                .WithMany(t => t.Schedules)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.TimeSlot)
                .WithMany(ts => ts.Schedules)
                .HasForeignKey(e => e.TimeSlotId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Room)
                .WithMany(r => r.Schedules)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TimeSlot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Label).HasMaxLength(100);
            entity.HasOne(e => e.Framework)
                .WithMany(f => f.GeneratedTimeSlots)
                .HasForeignKey(e => e.TimetableFrameworkId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<TimetableFramework>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.HasMany(e => e.Breaks)
                .WithOne(b => b.Framework)
                .HasForeignKey(b => b.TimetableFrameworkId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BreakDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Label).HasMaxLength(100);
        });

        modelBuilder.Entity<TeacherAvailability>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Teacher)
                .WithMany(t => t.Availabilities)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.TimeSlot)
                .WithMany(ts => ts.Availabilities)
                .HasForeignKey(e => e.TimeSlotId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Teacher>()
            .HasMany(t => t.Subjects)
            .WithMany(s => s.Teachers);

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
            entity.HasOne(e => e.Parent)
                .WithMany(p => p.Invoices)
                .HasForeignKey(e => e.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Student)
                .WithMany(s => s.Invoices)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).HasMaxLength(20).HasDefaultValue("#6366f1");
            entity.HasOne(e => e.RequiredRoom)
                .WithMany()
                .HasForeignKey(e => e.RequiredRoomId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Curriculum>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Subject)
                .WithMany(s => s.Curriculums)
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Building).HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.AnonymousGroup).HasMaxLength(100);
        });

        modelBuilder.Entity<ClassUnit>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Classroom)
                .WithMany()
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Teacher)
                .WithMany()
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(e => e.PreferredRoom)
                .WithMany()
                .HasForeignKey(e => e.PreferredRoomId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ScheduleConstraint>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Parameters).HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Weight).HasDefaultValue(5);
            entity.HasOne(e => e.Teacher)
                .WithMany()
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Subject)
                .WithMany()
                .HasForeignKey(e => e.SubjectId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Classroom)
                .WithMany()
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
