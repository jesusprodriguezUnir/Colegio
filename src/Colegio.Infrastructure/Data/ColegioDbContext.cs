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
            entity.Property(e => e.Subject).IsRequired().HasMaxLength(200);
            entity.HasOne(e => e.Classroom)
                .WithMany(c => c.Schedules)
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Teacher)
                .WithMany(t => t.Schedules)
                .HasForeignKey(e => e.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

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
    }
}
