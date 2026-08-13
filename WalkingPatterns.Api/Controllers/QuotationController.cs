using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using WalkingPatterns.Api.Data;

namespace WalkingPatterns.Api.Controllers;

[ApiController]
[Route("api/projects")]
public class QuotationController : ControllerBase
{
    private readonly AppDbContext _context;
    public QuotationController(AppDbContext context) => _context = context;

    [HttpGet("{projectId:int}/quotation")]
    public async Task<IActionResult> Generate(int projectId)
    {
        var project = await _context.ProjectVersionDetails.Include(item => item.Client).SingleOrDefaultAsync(item => item.Id == projectId);
        if (project == null) return NotFound(new { message = "Project not found." });
        var details = await _context.ProjectDetails.AsNoTracking().Where(item => item.ProjectId == projectId).ToListAsync();
        var orders = await _context.OrderDetails.AsNoTracking().Where(item => item.ProjectVersionDetailsId == projectId && item.ProjectId == projectId).ToListAsync();
        var total = orders.Sum(item => item.TotalPrice);
        var discount = Math.Clamp(project.DiscountAmount, 0, total);
        var discounted = Math.Max(0, total - discount);
        var document = Document.Create(container => container.Page(page => { page.Margin(30); page.DefaultTextStyle(x => x.FontSize(9)); page.Header().Element(header => Header(header, project)); page.Content().Column(column => { column.Spacing(12); column.Item().Text("Order Quotation").FontSize(18).Bold().AlignCenter(); column.Item().Element(c => ClientSection(c, project)); column.Item().Element(c => SummarySection(c, details)); column.Item().Element(c => PriceSection(c, total, discount, discounted)); column.Item().PageBreak(); DetailedSections(column, orders); column.Item().PageBreak(); Terms(column); }); page.Footer().AlignCenter().Text("Walking Patterns"); }));
        return File(document.GeneratePdf(), "application/pdf", $"{Safe(project.Client.ClientName)}_{Safe(project.VersionNumber)}.pdf");
    }

    private static void Header(IContainer c, Models.ProjectVersionDetails p) => c.Row(r => { r.RelativeItem().Text("Walking Patterns").Bold().FontSize(14); r.RelativeItem().AlignRight().Column(x => { x.Item().Text("Architect: Sahana S"); x.Item().Text("sahanasathish@walkingpatterns.com"); x.Item().Text("9611917967"); x.Item().Text($"Date: {DateTime.Now:dd/MM/yyyy}"); x.Item().Text($"Version: {p.VersionNumber}"); }); });
    private static void ClientSection(IContainer c, Models.ProjectVersionDetails p) => c.Column(x => { x.Item().Background(Colors.Black).Padding(5).Text("Client Details").FontColor(Colors.White).Bold(); x.Item().Text($"Client Name: {p.Client.ClientName}"); x.Item().Text($"Email: {p.Client.Email ?? "N/A"}"); x.Item().Text($"Phone: {p.Client.Phone ?? "N/A"}"); x.Item().Text($"Address: {p.Client.Address ?? "N/A"}"); });
    private static void SummarySection(IContainer c, List<Models.ProjectDetails> d) => c.Column(x => { x.Item().Background(Colors.Black).Padding(5).Text("Project Details").FontColor(Colors.White).Bold(); x.Item().Table(t => { t.ColumnsDefinition(cd => { for (var i = 0; i < 5; i++) cd.RelativeColumn(); }); foreach (var h in new[] { "Room Name", "Woodwork", "Accessories", "Services", "Total" }) t.Cell().Element(Cell).Text(h).Bold(); foreach (var g in d.GroupBy(i => i.RoomName)) { var a = g.Sum(i => Parse(i.Accessories)); var s = g.Sum(i => Parse(i.Services)); var total = g.Sum(i => Parse(i.Total)); foreach (var v in new[] { g.Key, Money(total - a - s), Money(a), Money(s), Money(total) }) t.Cell().Element(Cell).Text(v); } }); });
    private static void PriceSection(IContainer c, double t, double d, double net) => c.Column(x => { x.Item().Text("Price Summary").Bold().FontSize(13); x.Item().Text($"Total Amount: {Money(t)}"); x.Item().Text($"Discount Amount: {Money(d)}"); x.Item().Text($"Grand Total / Discounted Total: {Money(net)}").Bold(); });
    private static void DetailedSections(ColumnDescriptor c, List<Models.OrderDetails> orders) { var order = new[] { "KitchenUtility", "Bedroom", "Other Woodwork", "HDS", "Other" }; foreach (var g in orders.GroupBy(i => Module(i.UtilityNameOld)).OrderBy(g => Array.IndexOf(order, g.Key))) { c.Item().Text($"{g.Key} Summary").Bold().FontSize(14); foreach (var u in g.GroupBy(i => i.UtilityName)) { c.Item().Text(u.Key ?? "Other").Underline(); c.Item().Table(t => { t.ColumnsDefinition(cd => { for (var i = 0; i < 6; i++) cd.RelativeColumn(); }); foreach (var h in new[] { "Product Name", "Finish", "Accessories & Quantity", "Additional Items & Quantity", "Dimensions", "Total" }) t.Cell().Element(Cell).Text(h).Bold(); foreach (var i in u) foreach (var v in new[] { i.Parent ?? "", i.Materials ?? "", Pair(i.Accessories, i.Quantities), Pair(i.AdditionalItemName, i.AdditionalItemsQuantities), $"{i.Width} x {i.Height} x {i.Depth}", Money(i.TotalPrice) }) t.Cell().Element(Cell).Text(v); }); } } }
    private static void Terms(ColumnDescriptor c) => c.Item().Text("Terms and Conditions\n1. The quotation is an initial estimate and is valid for 4 months.\n2. Exact price depends on measurements, scope and design/material changes.\n3. Estimate variation may be approximately 10-15%.\n4. Measurement-based design development requires 10% of this estimate.\n5. Civil, plumbing, gas-piping and electrical work may be excluded.\n6. Post-installation cleaning services will be done at site.\n7. Mechanized cleaning is out of scope.\n8. Payment structure: 10%, 15%, 25%, and remaining 50%.\n9. Woodwork is covered under 10 years warranty.\n10. Accessories follow manufacturer warranties.\n11. Mirror, glass, countertop, tiles and civil works are as-is.\n12. Cancellation policy applies by design stage.\n13. Associated brands include Centuryply, Greenply, Hettich, Hafele, Saint Gobain and Asian Paints.\n\nBank Details\nAccount Name: Sandeep K\nBank: ICICI\nAccount Number: 777701905796\nIFSC: ICIC0006254\nBranch: Chamrajpete");
    private static IContainer Cell(IContainer c) => c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4);
    private static double Parse(string? v) => double.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var n) ? n : 0;
    private static string Money(double v) => $"Rs. {v.ToString("N2", CultureInfo.GetCultureInfo("en-IN"))}";
    private static string Pair(string? v, string? q) { var a = (v ?? "").Split(','); var b = (q ?? "").Split(','); var result = new List<string>(); for (var i = 0; i < a.Length; i++) if (!string.IsNullOrWhiteSpace(a[i]) && a[i] != "0") result.Add($"{a[i].Trim()} - {b.ElementAtOrDefault(i)?.Trim() ?? "N/A"}"); return result.Count == 0 ? "N/A" : string.Join("\n", result); }
    private static string Module(string? v) => v?.StartsWith("KitchenUtility", StringComparison.OrdinalIgnoreCase) == true ? "KitchenUtility" : v?.StartsWith("Bedroom", StringComparison.OrdinalIgnoreCase) == true ? "Bedroom" : v?.StartsWith("Other Woodwork", StringComparison.OrdinalIgnoreCase) == true ? "Other Woodwork" : v?.StartsWith("HDS", StringComparison.OrdinalIgnoreCase) == true ? "HDS" : "Other";
    private static string Safe(string v) { var invalid = Path.GetInvalidFileNameChars(); return new string(v.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()); }
}
