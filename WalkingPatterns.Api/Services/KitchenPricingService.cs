using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using WalkingPatterns.Api.Data;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Models;

namespace WalkingPatterns.Api.Services;

public class KitchenPricingService : IKitchenPricingService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public KitchenPricingService(AppDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public KitchenPricingResponse GetPricing()
    {
        var path = Path.Combine(_environment.ContentRootPath, "Pricing", "PricingKitchenData.json");
        using var stream = File.OpenRead(path);
        return JsonSerializer.Deserialize<KitchenPricingResponse>(stream, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new KitchenPricingResponse();
    }

    public async Task<KitchenItemResponse?> CalculateAndSaveAsync(int projectId, KitchenItemRequest request)
    {
        var project = await _context.ProjectVersionDetails.SingleOrDefaultAsync(item => item.Id == projectId);
        if (project == null)
            return null;

        var pricing = GetPricing();
        var errors = new Dictionary<string, string[]>();
        var parent = request.Parent?.Trim() ?? string.Empty;
        var utilityName = request.UtilityName?.Trim() ?? string.Empty;
        var material = request.Materials?.Trim() ?? string.Empty;

        if (!pricing.ParentOptions.TryGetValue(parent, out var parentOptions))
            errors["parent"] = new[] { "Parent is not available in kitchen pricing." };
        if (!pricing.Materials.TryGetValue(material, out var materialRate))
            errors["materials"] = new[] { "Material is not available in kitchen pricing." };
        if (string.IsNullOrWhiteSpace(utilityName))
            errors["utilityName"] = new[] { "Utility name is required." };

        if (!TryDimension(request.Width, "width", errors, out var widthMm) ||
            !TryDimension(request.Height, "height", errors, out var heightMm) ||
            !TryDimension(request.Depth, "depth", errors, out var depthMm))
            return errors.Count == 0 ? throw new InvalidOperationException() : throw new KitchenValidationException(errors);

        if (request.Accessories.Count != request.Quantities.Count)
            errors["quantities"] = new[] { "Accessories and quantities must have the same count." };

        var accessoryTotal = 0d;
        for (var index = 0; index < request.Accessories.Count && index < request.Quantities.Count; index++)
        {
            var accessoryName = request.Accessories[index].Trim();
            if (!int.TryParse(request.Quantities[index], NumberStyles.None, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0)
            {
                errors[$"quantities[{index}]"] = new[] { "Accessory quantity must be a positive integer." };
                continue;
            }

            var option = parentOptions?.FirstOrDefault(item => item.Name.Trim() == accessoryName);
            if (option == null)
                errors[$"accessories[{index}]"] = new[] { "Accessory is not available for the selected parent." };
            else
                accessoryTotal += option.Price * quantity;
        }

        var additionalTotal = 0d;
        foreach (var item in request.AdditionalItems)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || item.Amount < 0 || item.Quantity <= 0)
                errors["additionalItems"] = new[] { "Additional item name, amount, and quantity are invalid." };
            else
                additionalTotal += item.Amount * item.Quantity;
        }

        if (errors.Count > 0)
            throw new KitchenValidationException(errors);

        var widthFt = Math.Round(widthMm / 304.8, 1);
        var heightFt = Math.Round(heightMm / 304.8, 1);
        var depthFt = Math.Round(depthMm / 304.8, 1);
        var totalPrice = Math.Round(widthFt * heightFt * depthFt * materialRate + accessoryTotal + additionalTotal, 2);
        var additionalNames = request.AdditionalItems.Select(item => item.Name!.Trim()).ToArray();
        var additionalAmounts = request.AdditionalItems.Select(item => item.Amount.ToString(CultureInfo.InvariantCulture)).ToArray();
        var additionalQuantities = request.AdditionalItems.Select(item => item.Quantity.ToString(CultureInfo.InvariantCulture)).ToArray();

        var entity = new KitchenPriceDetails
        {
            Parent = parent, Width = request.Width!.Trim(), Height = request.Height!.Trim(), Depth = request.Depth!.Trim(),
            Materials = material, Accessories = string.Join(",", request.Accessories), Quantities = string.Join(",", request.Quantities),
            UtilityName = utilityName, UtilityNameOld = string.IsNullOrWhiteSpace(request.UtilityNameOld) ? "KitchenUtility" : request.UtilityNameOld.Trim(),
            AdditionalItemName = string.Join(",", additionalNames), AdditionalItemsAmounts = string.Join(",", additionalAmounts),
            AdditionalItemsQuantities = string.Join(",", additionalQuantities), ProjectName = project.ProjectName,
            MaterialTotal = materialRate, AccessoriesTotal = accessoryTotal, AdditionalItemsTotal = additionalTotal,
            TotalPrice = totalPrice, CreatedAt = DateTime.Now
        };

        _context.KitchenPriceDetails.Add(entity);
        await _context.SaveChangesAsync();
        return new KitchenItemResponse
        {
            Id = entity.Id, ProjectId = projectId, Parent = entity.Parent, UtilityName = entity.UtilityName,
            UtilityNameOld = entity.UtilityNameOld, Width = entity.Width, Height = entity.Height, Depth = entity.Depth,
            Materials = entity.Materials, Accessories = entity.Accessories, Quantities = entity.Quantities,
            AdditionalItems = entity.AdditionalItemName, MaterialTotal = entity.MaterialTotal ?? 0,
            AccessoriesTotal = entity.AccessoriesTotal ?? 0, AdditionalItemsTotal = entity.AdditionalItemsTotal ?? 0,
            TotalPrice = entity.TotalPrice ?? 0, CreatedAt = entity.CreatedAt
        };
    }

    private static bool TryDimension(string? value, string key, Dictionary<string, string[]> errors, out double result)
    {
        if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out result) || result <= 0)
        {
            errors[key] = new[] { "Dimension must be a valid positive number in millimetres." };
            return false;
        }
        return true;
    }
}

public sealed class KitchenValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }
    public KitchenValidationException(Dictionary<string, string[]> errors) : base("Kitchen input validation failed.") => Errors = errors;
}
