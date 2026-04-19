using Colegio.Domain.Entities;

namespace Colegio.Domain.Services;

public class InvoiceService
{
    public Invoice CreateMonthlyInvoice(Student student, Parent parent, decimal amount, DateTime issueDate)
    {
        return new Invoice
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            ParentId = parent.Id,
            IssueDate = issueDate,
            DueDate = issueDate.AddDays(30),
            TotalAmount = amount,
            Status = InvoiceStatus.Pending,
            Concept = InvoiceConcept.Monthly
        };
    }

    public Invoice CreateLunchInvoice(Student student, Parent parent, decimal amount, DateTime issueDate)
    {
        return new Invoice
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            ParentId = parent.Id,
            IssueDate = issueDate,
            DueDate = issueDate.AddDays(15),
            TotalAmount = amount,
            Status = InvoiceStatus.Pending,
            Concept = InvoiceConcept.Lunch
        };
    }

    public Invoice CreateExtracurricularInvoice(Student student, Parent parent, decimal amount, DateTime issueDate)
    {
        return new Invoice
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            ParentId = parent.Id,
            IssueDate = issueDate,
            DueDate = issueDate.AddDays(7),
            TotalAmount = amount,
            Status = InvoiceStatus.Pending,
            Concept = InvoiceConcept.Extracurricular
        };
    }

    public void MarkAsPaid(Invoice invoice)
    {
        invoice.Status = InvoiceStatus.Paid;
    }

    public bool IsOverdue(Invoice invoice)
    {
        return invoice.Status == InvoiceStatus.Pending && invoice.DueDate < DateTime.UtcNow;
    }

    public decimal CalculateTotalPending(IEnumerable<Invoice> invoices)
    {
        return invoices
            .Where(i => i.Status == InvoiceStatus.Pending)
            .Sum(i => i.TotalAmount);
    }

    public decimal CalculateTotalPaid(IEnumerable<Invoice> invoices)
    {
        return invoices
            .Where(i => i.Status == InvoiceStatus.Paid)
            .Sum(i => i.TotalAmount);
    }
}