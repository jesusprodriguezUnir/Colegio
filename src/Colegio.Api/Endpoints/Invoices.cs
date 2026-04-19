using Colegio.Domain.Entities;
using Colegio.Domain.Services;
using Colegio.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Colegio.Api.Endpoints;

public static class InvoicesEndpoints
{
    private static readonly InvoiceService _invoiceService = new();

    public static void MapInvoicesEndpoints(this WebApplication app)
    {
        app.MapGet("/api/invoices", GetAllInvoices);
        app.MapGet("/api/invoices/{id}", GetInvoiceById);
        app.MapGet("/api/invoices/parent/{parentId}", GetInvoicesByParent);
        app.MapGet("/api/invoices/student/{studentId}", GetInvoicesByStudent);
        app.MapGet("/api/invoices/summary", GetInvoiceSummary);
        app.MapPost("/api/invoices", CreateInvoice);
        app.MapPost("/api/invoices/generate-monthly", GenerateMonthlyInvoices);
        app.MapPut("/api/invoices/{id}", UpdateInvoice);
        app.MapPut("/api/invoices/{id}/pay", PayInvoice);
        app.MapDelete("/api/invoices/{id}", DeleteInvoice);
    }

    private static async Task<IResult> GetAllInvoices(ColegioDbContext db)
    {
        var invoices = await db.Invoices
            .AsNoTracking()
            .Include(i => i.Student)
            .Include(i => i.Parent)
            .ToListAsync();
        return Results.Ok(invoices);
    }

    private static async Task<IResult> GetInvoiceById(ColegioDbContext db, Guid id)
    {
        var invoice = await db.Invoices
            .AsNoTracking()
            .Include(i => i.Student)
            .Include(i => i.Parent)
            .FirstOrDefaultAsync(i => i.Id == id);
        return invoice is null ? Results.NotFound() : Results.Ok(invoice);
    }

    private static async Task<IResult> GetInvoicesByParent(ColegioDbContext db, Guid parentId)
    {
        var invoices = await db.Invoices
            .AsNoTracking()
            .Where(i => i.ParentId == parentId)
            .Include(i => i.Student)
            .ToListAsync();
        return Results.Ok(invoices);
    }

    private static async Task<IResult> GetInvoicesByStudent(ColegioDbContext db, Guid studentId)
    {
        var invoices = await db.Invoices
            .AsNoTracking()
            .Where(i => i.StudentId == studentId)
            .Include(i => i.Parent)
            .ToListAsync();
        return Results.Ok(invoices);
    }

    private static async Task<IResult> GetInvoiceSummary(ColegioDbContext db)
    {
        var allInvoices = await db.Invoices.AsNoTracking().ToListAsync();

        var summary = new
        {
            TotalPending = _invoiceService.CalculateTotalPending(allInvoices),
            TotalPaid = _invoiceService.CalculateTotalPaid(allInvoices),
            PendingCount = allInvoices.Count(i => i.Status == InvoiceStatus.Pending),
            PaidCount = allInvoices.Count(i => i.Status == InvoiceStatus.Paid),
            OverdueCount = allInvoices.Count(i => _invoiceService.IsOverdue(i))
        };

        return Results.Ok(summary);
    }

    private static async Task<IResult> GenerateMonthlyInvoices(ColegioDbContext db, GenerateMonthlyRequest request)
    {
        var students = await db.Students
            .Include(s => s.StudentParents)
                .ThenInclude(sp => sp.Parent)
            .ToListAsync();

        var newInvoices = new List<Invoice>();
        var issueDate = request.IssueDate ?? DateTime.UtcNow;

        foreach (var student in students)
        {
            foreach (var sp in student.StudentParents)
            {
                var invoice = _invoiceService.CreateMonthlyInvoice(student, sp.Parent, request.Amount, issueDate);
                newInvoices.Add(invoice);
            }
        }

        db.Invoices.AddRange(newInvoices);
        await db.SaveChangesAsync();

        return Results.Created("/api/invoices", new { Count = newInvoices.Count });
    }

    private static async Task<IResult> CreateInvoice(ColegioDbContext db, Invoice invoice)
    {
        invoice.Id = Guid.NewGuid();
        db.Invoices.Add(invoice);
        await db.SaveChangesAsync();
        return Results.Created($"/api/invoices/{invoice.Id}", invoice);
    }

    private static async Task<IResult> UpdateInvoice(ColegioDbContext db, Guid id, Invoice updated)
    {
        var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == id);
        if (invoice is null) return Results.NotFound();

        invoice.ParentId = updated.ParentId;
        invoice.StudentId = updated.StudentId;
        invoice.IssueDate = updated.IssueDate;
        invoice.DueDate = updated.DueDate;
        invoice.TotalAmount = updated.TotalAmount;
        invoice.Status = updated.Status;
        invoice.Concept = updated.Concept;

        db.Invoices.Update(invoice);
        await db.SaveChangesAsync();
        return Results.Ok(invoice);
    }

    private static async Task<IResult> PayInvoice(ColegioDbContext db, Guid id)
    {
        var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == id);
        if (invoice is null) return Results.NotFound();

        invoice.Status = InvoiceStatus.Paid;
        db.Invoices.Update(invoice);
        await db.SaveChangesAsync();
        return Results.Ok(invoice);
    }

    private static async Task<IResult> DeleteInvoice(ColegioDbContext db, Guid id)
    {
        var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == id);
        if (invoice is null) return Results.NotFound();

        db.Invoices.Remove(invoice);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
}

public class GenerateMonthlyRequest
{
    public decimal Amount { get; set; } = 350.00m;
    public DateTime? IssueDate { get; set; }
}