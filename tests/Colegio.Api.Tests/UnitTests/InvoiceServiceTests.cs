using Colegio.Domain.Entities;
using Colegio.Domain.Services;
using FluentAssertions;

namespace Colegio.Api.Tests.UnitTests;

public class InvoiceServiceTests
{
    private readonly InvoiceService _invoiceService = new();

    [Fact]
    public void CreateMonthlyInvoice_ShouldCreateInvoiceWithCorrectProperties()
    {
        var student = new Student { Id = Guid.NewGuid() };
        var parent = new Parent { Id = Guid.NewGuid() };
        var amount = 350.00m;
        var issueDate = new DateTime(2026, 4, 1);

        var invoice = _invoiceService.CreateMonthlyInvoice(student, parent, amount, issueDate);

        invoice.Should().NotBeNull();
        invoice.Id.Should().NotBeEmpty();
        invoice.StudentId.Should().Be(student.Id);
        invoice.ParentId.Should().Be(parent.Id);
        invoice.TotalAmount.Should().Be(amount);
        invoice.Status.Should().Be(InvoiceStatus.Pending);
        invoice.Concept.Should().Be(InvoiceConcept.Monthly);
        invoice.IssueDate.Should().Be(issueDate);
        invoice.DueDate.Should().Be(issueDate.AddDays(30));
    }

    [Fact]
    public void CreateLunchInvoice_ShouldSetCorrectDueDate()
    {
        var student = new Student { Id = Guid.NewGuid() };
        var parent = new Parent { Id = Guid.NewGuid() };
        var amount = 120.00m;
        var issueDate = new DateTime(2026, 4, 1);

        var invoice = _invoiceService.CreateLunchInvoice(student, parent, amount, issueDate);

        invoice.DueDate.Should().Be(issueDate.AddDays(15));
        invoice.Concept.Should().Be(InvoiceConcept.Lunch);
    }

    [Fact]
    public void CreateExtracurricularInvoice_ShouldSetShorterDueDate()
    {
        var student = new Student { Id = Guid.NewGuid() };
        var parent = new Parent { Id = Guid.NewGuid() };
        var amount = 50.00m;
        var issueDate = new DateTime(2026, 4, 1);

        var invoice = _invoiceService.CreateExtracurricularInvoice(student, parent, amount, issueDate);

        invoice.DueDate.Should().Be(issueDate.AddDays(7));
        invoice.Concept.Should().Be(InvoiceConcept.Extracurricular);
    }

    [Fact]
    public void MarkAsPaid_ShouldChangeStatusToPaid()
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Status = InvoiceStatus.Pending
        };

        _invoiceService.MarkAsPaid(invoice);

        invoice.Status.Should().Be(InvoiceStatus.Paid);
    }

    [Fact]
    public void IsOverdue_ShouldReturnTrueForPendingAndPastDueDate()
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Status = InvoiceStatus.Pending,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        var result = _invoiceService.IsOverdue(invoice);

        result.Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_ShouldReturnFalseForPaidInvoice()
    {
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            Status = InvoiceStatus.Paid,
            DueDate = DateTime.UtcNow.AddDays(-1)
        };

        var result = _invoiceService.IsOverdue(invoice);

        result.Should().BeFalse();
    }

    [Fact]
    public void CalculateTotalPending_ShouldSumOnlyPendingInvoices()
    {
        var invoices = new List<Invoice>
        {
            new() { Id = Guid.NewGuid(), Status = InvoiceStatus.Pending, TotalAmount = 100.00m },
            new() { Id = Guid.NewGuid(), Status = InvoiceStatus.Paid, TotalAmount = 200.00m },
            new() { Id = Guid.NewGuid(), Status = InvoiceStatus.Pending, TotalAmount = 150.00m }
        };

        var total = _invoiceService.CalculateTotalPending(invoices);

        total.Should().Be(250.00m);
    }

    [Fact]
    public void CalculateTotalPaid_ShouldSumOnlyPaidInvoices()
    {
        var invoices = new List<Invoice>
        {
            new() { Id = Guid.NewGuid(), Status = InvoiceStatus.Pending, TotalAmount = 100.00m },
            new() { Id = Guid.NewGuid(), Status = InvoiceStatus.Paid, TotalAmount = 200.00m },
            new() { Id = Guid.NewGuid(), Status = InvoiceStatus.Paid, TotalAmount = 150.00m }
        };

        var total = _invoiceService.CalculateTotalPaid(invoices);

        total.Should().Be(350.00m);
    }
}