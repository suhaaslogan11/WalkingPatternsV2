using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WalkingPatterns.Api.Data;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Models;

namespace WalkingPatterns.Api.Services;

public class BedroomPricingService : IBedroomPricingService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public BedroomPricingService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public BedroomPricingResponse GetPricing()
    {
        var path = Path.Combine(
            _environment.ContentRootPath,
            "Pricing",
            "PricingBedroomData.json"
        );

        using var stream = File.OpenRead(path);

        return JsonSerializer.Deserialize<BedroomPricingResponse>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            }
        ) ?? new BedroomPricingResponse();
    }

    public async Task<BedroomItemResponse?> CalculateAndSaveAsync(int projectId,BedroomItemRequest request)
    {
        var project = await _context.ProjectVersionDetails.SingleOrDefaultAsync(item => item.Id == projectId);

        if (project == null)
            return null;

        var pricing = GetPricing();

        var errors = new Dictionary<string, string[]>();

        var parent = request.Parent?.Trim() ?? string.Empty;
        var core = request.CoreMaterial?.Trim() ?? string.Empty;
        var shutter = request.ShutterMaterial?.Trim() ?? string.Empty;
        var utility = request.UtilityName?.Trim() ?? string.Empty;

        pricing.PricingData.TryGetValue(parent, out var coreOptions);

        if (coreOptions == null)
        {
            errors["parent"] = new[]
            {
            "Parent is not available in bedroom pricing."
        };
        }
        else if (!coreOptions.TryGetValue(core, out var shutterOptions))
        {
            errors["coreMaterial"] = new[]
            {
            "Core material is not available for the selected parent."
        };
        }
        else if (!shutterOptions.TryGetValue(shutter, out _))
        {
            errors["shutterMaterial"] = new[]
            {
            "Shutter material is not available for the selected parent and core material."
        };
        }

        if (string.IsNullOrWhiteSpace(utility))
        {
            errors["utilityName"] = new[]
            {
            "Utility name is required."
        };
        }

        var validDimensions = TryDimension(request.Width,"width",errors,out var width) & TryDimension(request.Height,"height",errors,out var height) & TryDimension(request.Depth,"depth",errors,out var depth);

        request.AdditionalItems ??= new List<BedroomAdditionalItemRequest>();

        var validAdditionalItems = request.AdditionalItems.Where(item => !string.IsNullOrWhiteSpace(item.Name) || item.Amount != 0).ToList();

        var additionalTotal = 0d;

        foreach (var item in validAdditionalItems)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || item.Amount < 0 || item.Quantity <= 0)
            {
                errors["additionalItems"] = new[]
                {
                "Additional item name, amount, and quantity are invalid."
            };
            }
            else
            {
                additionalTotal += item.Amount * item.Quantity;
            }
        }

        if (errors.Count > 0 || !validDimensions)
        {
            throw new BedroomValidationException(errors);
        }

        var materialTotal = pricing.PricingData[parent][core][shutter];

        var widthFt = Math.Round(width / 304.8, 1);

        var heightFt = Math.Round(height / 304.8, 1);

        var depthFt = Math.Round(depth / 304.8, 1);

        var total = Math.Round(widthFt * heightFt * depthFt * materialTotal + additionalTotal,2);

        var additionalNames = validAdditionalItems.Select(item => item.Name!.Trim()).ToArray();

        var additionalAmounts = validAdditionalItems.Select(item => item.Amount.ToString(CultureInfo.InvariantCulture)).ToArray();

        var additionalQuantities = validAdditionalItems.Select(item => item.Quantity.ToString(CultureInfo.InvariantCulture)).ToArray();

        var entity = new BedrromPriceDetails
        {
            Parent = parent,

            Width = request.Width!.Trim(),
            Height = request.Height!.Trim(),
            Depth = request.Depth!.Trim(),

            Materials = $"{core}, {shutter}",

            UtilityName = utility,

            UtilityNameOld = string.IsNullOrWhiteSpace(request.UtilityNameOld) ? "Bedroom" : request.UtilityNameOld.Trim(),
            AdditionalItemName = string.Join(",", additionalNames),

            AdditionalItemsAmounts = string.Join(",", additionalAmounts),

            AdditionalItemsQuantities = string.Join(",", additionalQuantities),

            ProjectName = project.ProjectName,

            MaterialTotal = materialTotal,

            AdditionalItemsTotal = additionalTotal,

            TotalPrice = total,

            CreatedAt = DateTime.Now
        };

        _context.BedromPriceDetails.Add(entity);

        await _context.SaveChangesAsync();

        return new BedroomItemResponse
        {
            Id = entity.Id,

            ProjectId = projectId,

            Parent = entity.Parent,

            UtilityName =
                entity.UtilityName,

            UtilityNameOld =
                entity.UtilityNameOld,

            Width =
                entity.Width,

            Height =
                entity.Height,

            Depth =
                entity.Depth,

            Materials =
                entity.Materials,

            AdditionalItems =
                entity.AdditionalItemName,

            MaterialTotal =
                entity.MaterialTotal,

            AccessoriesTotal = 0,

            AdditionalItemsTotal =
                entity.AdditionalItemsTotal,

            TotalPrice =
                entity.TotalPrice,

            CreatedAt =
                entity.CreatedAt
        };
    }

    public async Task<BedroomItemResponse?> UpdateOrderAsync(int projectId, int orderId, BedroomItemRequest request)
    {
        var project = await _context.ProjectVersionDetails.FindAsync(projectId);
        var order = await _context.OrderDetails.SingleOrDefaultAsync(x => x.OrderId == orderId && x.ProjectId == projectId && x.ProjectVersionDetailsId == projectId);
        if (project == null || order == null || !string.Equals(order.UtilityNameOld, "Bedroom", StringComparison.OrdinalIgnoreCase)) return null;
        await using var tx = await _context.Database.BeginTransactionAsync();
        var created = await CalculateAndSaveAsync(projectId, request);
        if (created == null) return null;
        var temp = await _context.BedromPriceDetails.FindAsync(created.Id);
        if (temp == null) return null;
        order.Parent = temp.Parent; order.UtilityName = temp.UtilityName; order.UtilityNameOld = temp.UtilityNameOld; order.Width = temp.Width; order.Height = temp.Height; order.Depth = temp.Depth; order.Materials = temp.Materials; order.Accessories = null; order.Quantities = null; order.AdditionalItemName = temp.AdditionalItemName; order.AdditionalItemsAmounts = temp.AdditionalItemsAmounts; order.AdditionalItemsQuantities = temp.AdditionalItemsQuantities; order.MaterialTotal = temp.MaterialTotal; order.AccessoriesTotal = 0; order.AdditionalItemsTotal = temp.AdditionalItemsTotal; order.TotalPrice = temp.TotalPrice;
        _context.BedromPriceDetails.Remove(temp);
        await SyncTotals(project, order.UtilityName ?? string.Empty);
        await _context.SaveChangesAsync(); await tx.CommitAsync();
        created.Id = order.OrderId;
        return created;
    }

    private async Task SyncTotals(ProjectVersionDetails project, string room)
    {
        var orders = await _context.OrderDetails.Where(x => x.ProjectId == project.Id && x.ProjectVersionDetailsId == project.Id && x.UtilityName == room).ToListAsync();
        var details = await _context.ProjectDetails.Where(x => x.ProjectId == project.Id && x.RoomName == room).ToListAsync();
        if (details.Count > 0) { details[0].Woodwork = orders.Sum(x => x.MaterialTotal).ToString(CultureInfo.InvariantCulture); details[0].Accessories = "0"; details[0].Services = orders.Sum(x => x.AdditionalItemsTotal).ToString(CultureInfo.InvariantCulture); details[0].Total = orders.Sum(x => x.TotalPrice).ToString(CultureInfo.InvariantCulture); }
        var all = await _context.ProjectDetails.Where(x => x.ProjectId == project.Id).ToListAsync(); var total = all.Sum(x => double.TryParse(x.Total, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0d); project.GrandTotal = total; project.DiscountAmount = Math.Min(Math.Max(project.DiscountAmount, 0), total); project.DiscountedTotal = total - project.DiscountAmount;
    }

    private static bool TryDimension(string? value, string key, Dictionary<string, string[]> errors, out double result)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) || result <= 0)
        { errors[key] = new[] { "Dimension must be a valid positive number in millimetres." }; return false; }
        return true;
    }
}

public sealed class BedroomValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }
    public BedroomValidationException(Dictionary<string, string[]> errors) : base("Bedroom input validation failed.") => Errors = errors;
}
