namespace Colegio.Domain.Entities;

public enum InvoiceStatus
{
    Pending,
    Paid
}

public enum InvoiceConcept
{
    Monthly,
    Lunch,
    Extracurricular
}

public class Invoice
{
    public Guid Id { get; set; }
    public Guid ParentId { get; set; }
    public Guid StudentId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime DueDate { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public InvoiceConcept Concept { get; set; }

    public Parent Parent { get; set; } = null!;
    public Student Student { get; set; } = null!;
}