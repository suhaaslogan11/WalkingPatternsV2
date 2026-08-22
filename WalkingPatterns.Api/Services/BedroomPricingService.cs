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
