namespace Colegio.Domain.Entities;

public enum RelationshipType
{
    Father,
    Mother,
    Guardian,
    Other
}

public class StudentParent
{
    public Guid StudentId { get; set; }
    public Guid ParentId { get; set; }
    public RelationshipType Relationship { get; set; }

    public Student Student { get; set; } = null!;
    public Parent Parent { get; set; } = null!;
}