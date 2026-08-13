using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WalkingPatterns.Api.Data;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Models;

namespace WalkingPatterns.Api.Services;

public class OtherWoodworkPricingService : IOtherWoodworkPricingService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    public OtherWoodworkPricingService(AppDbContext context, IWebHostEnvironment environment) { _context = context; _environment = environment; }

    public OtherWoodworkPricingResponse GetPricing()
    {
        var path = Path.Combine(_environment.ContentRootPath, "Pricing", "PricingOthersData.json");
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<OtherWoodworkPricingResponse>(stream, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
    }

    public async Task<OtherWoodworkItemResponse?> CalculateAndSaveAsync(int projectId, OtherWoodworkItemRequest request)
    {
        var project = await _context.ProjectVersionDetails.SingleOrDefaultAsync(item => item.Id == projectId);
        if (project == null) return null;
        var pricing = GetPricing();
        var errors = new Dictionary<string, string[]>();
        var parent = request.Parent?.Trim() ?? string.Empty;
        var core = request.CoreMaterial?.Trim() ?? string.Empty;
        var shutter = request.ShutterMaterial?.Trim() ?? string.Empty;
        var utility = request.UtilityName?.Trim() ?? string.Empty;
        pricing.PricingData.TryGetValue(parent, out var coreOptions);
        if (coreOptions == null) errors["parent"] = new[] { "Parent is not available in other woodwork pricing." };
        else if (!coreOptions.TryGetValue(core, out var shutterOptions)) errors["coreMaterial"] = new[] { "Core material is not available for the selected parent." };
        else if (!shutterOptions.ContainsKey(shutter)) errors["shutterMaterial"] = new[] { "Shutter material is not available for the selected parent and core material." };
        if (string.IsNullOrWhiteSpace(utility)) errors["utilityName"] = new[] { "Utility name is required." };
        var valid = TryDimension(request.Width, "width", errors, out var width) & TryDimension(request.Height, "height", errors, out var height) & TryDimension(request.Depth, "depth", errors, out var depth);
        var additionalTotal = 0d;
        foreach (var item in request.AdditionalItems)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || item.Amount < 0 || item.Quantity <= 0) errors["additionalItems"] = new[] { "Additional item name, amount, and quantity are invalid." };
            else additionalTotal += item.Amount * item.Quantity;
        }
        if (errors.Count > 0 || !valid) throw new OtherWoodworkValidationException(errors);
        var materialTotal = pricing.PricingData[parent][core][shutter];
        var total = Math.Round(Math.Round(width / 304.8, 1) * Math.Round(height / 304.8, 1) * Math.Round(depth / 304.8, 1) * materialTotal + additionalTotal, 2);
        var entity = new OtherWoodworkPriceDetails
        {
            Parent = parent, Width = request.Width!.Trim(), Height = request.Height!.Trim(), Depth = request.Depth!.Trim(), Materials = $"{core}, {shutter}", UtilityName = utility,
            UtilityNameOld = string.IsNullOrWhiteSpace(request.UtilityNameOld) ? "Other Woodwork" : request.UtilityNameOld.Trim(),
            AdditionalItemName = string.Join(",", request.AdditionalItems.Select(item => item.Name!.Trim())), AdditionalItemsAmounts = string.Join(",", request.AdditionalItems.Select(item => item.Amount.ToString(CultureInfo.InvariantCulture))), AdditionalItemsQuantities = string.Join(",", request.AdditionalItems.Select(item => item.Quantity.ToString(CultureInfo.InvariantCulture))),
            ProjectName = project.ProjectName, MaterialTotal = materialTotal, AdditionalItemsTotal = additionalTotal, TotalPrice = total, CreatedAt = DateTime.Now
        };
        _context.OtherWoodworkPriceDetails.Add(entity); await _context.SaveChangesAsync();
        return new OtherWoodworkItemResponse { Id = entity.Id, ProjectId = projectId, Parent = entity.Parent, UtilityName = entity.UtilityName, UtilityNameOld = entity.UtilityNameOld, Width = entity.Width, Height = entity.Height, Depth = entity.Depth, Materials = entity.Materials, AdditionalItems = entity.AdditionalItemName, MaterialTotal = entity.MaterialTotal, AdditionalItemsTotal = entity.AdditionalItemsTotal, TotalPrice = entity.TotalPrice, CreatedAt = entity.CreatedAt };
    }

    private static bool TryDimension(string? value, string key, Dictionary<string, string[]> errors, out double result) { if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) || result <= 0) { errors[key] = new[] { "Dimension must be a valid positive number in millimetres." }; return false; } return true; }
}

public sealed class OtherWoodworkValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }
    public OtherWoodworkValidationException(Dictionary<string, string[]> errors) : base("Other woodwork input validation failed.") => Errors = errors;
}
