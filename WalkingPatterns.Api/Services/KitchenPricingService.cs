using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WalkingPatterns.Api.Data;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Models;

namespace WalkingPatterns.Api.Services;

public class KitchenPricingService : IKitchenPricingService
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public KitchenPricingService(
        AppDbContext context,
        IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public KitchenPricingResponse GetPricing()
    {
        var path = Path.Combine(
            _environment.ContentRootPath,
            "Pricing",
            "PricingKitchenData.json"
        );

        using var stream = File.OpenRead(path);

        return JsonSerializer.Deserialize<KitchenPricingResponse>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            }
        ) ?? new KitchenPricingResponse();
    }

    public async Task<KitchenItemResponse?> CalculateAndSaveAsync(int projectId,KitchenItemRequest request)
    {
        var project = await _context.ProjectVersionDetails
            .SingleOrDefaultAsync(item => item.Id == projectId);

        if (project == null)
            return null;

        var pricing = GetPricing();

        var errors = new Dictionary<string, string[]>();

        var parent = request.Parent?.Trim() ?? string.Empty;
        var utilityName = request.UtilityName?.Trim() ?? string.Empty;
        var material = request.Materials?.Trim() ?? string.Empty;

        if (!pricing.ParentOptions.TryGetValue(parent,out var parentOptions))
        {
            errors["parent"] = new[]
            {
                "Parent is not available in kitchen pricing."
            };
        }

        if (!pricing.Materials.TryGetValue(material,out var materialRate))
        {
            errors["materials"] = new[]
            {
                "Material is not available in kitchen pricing."
            };
        }

        if (string.IsNullOrWhiteSpace(utilityName))
        {
            errors["utilityName"] = new[]
            {
                "Utility name is required."
            };
        }

        if (!TryDimension(request.Width,"width",errors,out var widthMm) || !TryDimension(request.Height,"height",errors,out var heightMm) || !TryDimension(request.Depth,"depth",errors,out var depthMm))
        {
            throw new KitchenValidationException(errors);
        }

        request.Accessories ??= new List<string>();
        request.Quantities ??= new List<string>();
        request.AdditionalItems ??= new List<KitchenAdditionalItemRequest>();

        if (request.Accessories.Count != request.Quantities.Count)
        {
            errors["quantities"] = new[]
            {
                "Accessories and quantities must have the same count."
            };
        }

        var accessoryTotal = 0d;

        for (var index = 0;index < request.Accessories.Count && index < request.Quantities.Count;index++)
        {
            var accessoryName =
                request.Accessories[index]?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(accessoryName))
                continue;

            if (!int.TryParse(request.Quantities[index],NumberStyles.None,CultureInfo.InvariantCulture,out var quantity) || quantity <= 0)
            {
                errors[$"quantities[{index}]"] = new[]
                {
                    "Accessory quantity must be a positive integer."
                };

                continue;
            }

            var option = parentOptions?.FirstOrDefault(item => item.Name.Trim() == accessoryName);

            if (option == null)
            {
                errors[$"accessories[{index}]"] = new[]
                {
                    "Accessory is not available for the selected parent."
                };
            }
            else
            {
                accessoryTotal += option.Price * quantity;
            }
        }

        var validAdditionalItems = request.AdditionalItems.Where(item => !string.IsNullOrWhiteSpace(item.Name) || item.Amount != 0).ToList();

        var additionalTotal = 0d;

        foreach (var item in validAdditionalItems)
        {
            if (string.IsNullOrWhiteSpace(item.Name)
                || item.Amount < 0
                || item.Quantity <= 0)
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

        if (errors.Count > 0)
        {
            throw new KitchenValidationException(errors);
        }

        var widthFt = Math.Round(widthMm / 304.8, 1);
        var heightFt = Math.Round(heightMm / 304.8, 1);
        var depthFt = Math.Round(depthMm / 304.8, 1);

        var totalPrice = Math.Round(widthFt * heightFt * depthFt * materialRate + accessoryTotal + additionalTotal,2);

        var additionalNames = validAdditionalItems.Select(item => item.Name!.Trim()).ToArray();

        var additionalAmounts = validAdditionalItems.Select(item => item.Amount.ToString(CultureInfo.InvariantCulture)).ToArray();

        var additionalQuantities = validAdditionalItems.Select(item => item.Quantity.ToString(CultureInfo.InvariantCulture)).ToArray();

        var entity = new KitchenPriceDetails
        {
            Parent = parent,

            Width = request.Width!.Trim(),
            Height = request.Height!.Trim(),
            Depth = request.Depth!.Trim(),

            Materials = material,

            Accessories = string.Join(
                ",",
                request.Accessories),

            Quantities = string.Join(
                ",",
                request.Quantities),

            UtilityName = utilityName,

            UtilityNameOld =
                string.IsNullOrWhiteSpace(request.UtilityNameOld)
                    ? "KitchenUtility"
                    : request.UtilityNameOld.Trim(),

            AdditionalItemName =
                string.Join(",", additionalNames),

            AdditionalItemsAmounts =
                string.Join(",", additionalAmounts),

            AdditionalItemsQuantities =
                string.Join(",", additionalQuantities),

            ProjectName = project.ProjectName,

            MaterialTotal = materialRate,

            AccessoriesTotal = accessoryTotal,

            AdditionalItemsTotal = additionalTotal,

            TotalPrice = totalPrice,

            CreatedAt = DateTime.Now
        };

        _context.KitchenPriceDetails.Add(entity);

        await _context.SaveChangesAsync();

        return new KitchenItemResponse
        {
            Id = entity.Id,

            ProjectId = projectId,

            Parent = entity.Parent,

            UtilityName = entity.UtilityName,

            UtilityNameOld = entity.UtilityNameOld,

            Width = entity.Width,

            Height = entity.Height,

            Depth = entity.Depth,

            Materials = entity.Materials,

            Accessories = entity.Accessories,

            Quantities = entity.Quantities,

            AdditionalItems =
                entity.AdditionalItemName,

            MaterialTotal =
                entity.MaterialTotal ?? 0,

            AccessoriesTotal =
                entity.AccessoriesTotal ?? 0,

            AdditionalItemsTotal =
                entity.AdditionalItemsTotal ?? 0,

            TotalPrice =
                entity.TotalPrice ?? 0,

            CreatedAt =
                entity.CreatedAt
        };
    }

    public async Task<KitchenItemResponse?> UpdateOrderAsync(int projectId, int orderId, KitchenItemRequest request)
    {
        var project = await _context.ProjectVersionDetails.SingleOrDefaultAsync(item => item.Id == projectId);
        if (project == null) return null;

        var order = await _context.OrderDetails.SingleOrDefaultAsync(item =>
            item.OrderId == orderId && item.ProjectVersionDetailsId == projectId && item.ProjectId == projectId);
        if (order == null || !string.Equals(order.UtilityNameOld, "KitchenUtility", StringComparison.OrdinalIgnoreCase))
            return null;

        var pricing = GetPricing();
        var errors = new Dictionary<string, string[]>();
        var parent = request.Parent?.Trim() ?? string.Empty;
        var utility = request.UtilityName?.Trim() ?? string.Empty;
        var material = request.Materials?.Trim() ?? string.Empty;
        if (!pricing.ParentOptions.TryGetValue(parent, out var parentOptions)) errors["parent"] = new[] { "Parent is not available in kitchen pricing." };
        if (!pricing.Materials.TryGetValue(material, out var materialRate)) errors["materials"] = new[] { "Material is not available in kitchen pricing." };
        if (string.IsNullOrWhiteSpace(utility)) errors["utilityName"] = new[] { "Utility name is required." };
        var validDimensions = TryDimension(request.Width, "width", errors, out var widthMm)
            & TryDimension(request.Height, "height", errors, out var heightMm)
            & TryDimension(request.Depth, "depth", errors, out var depthMm);
        request.Accessories ??= new List<string>(); request.Quantities ??= new List<string>(); request.AdditionalItems ??= new List<KitchenAdditionalItemRequest>();
        if (request.Accessories.Count != request.Quantities.Count) errors["quantities"] = new[] { "Accessories and quantities must have the same count." };
        var accessoryTotal = 0d;
        for (var i = 0; i < request.Accessories.Count && i < request.Quantities.Count; i++)
        {
            var accessory = request.Accessories[i]?.Trim() ?? string.Empty;
            if (!int.TryParse(request.Quantities[i], NumberStyles.None, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0) { errors[$"quantities[{i}]"] = new[] { "Accessory quantity must be a positive integer." }; continue; }
            var option = parentOptions?.FirstOrDefault(item => item.Name.Trim() == accessory);
            if (option == null) errors[$"accessories[{i}]"] = new[] { "Accessory is not available for the selected parent." }; else accessoryTotal += option.Price * quantity;
        }
        var additional = request.AdditionalItems.Where(item => !string.IsNullOrWhiteSpace(item.Name) || item.Amount != 0).ToList();
        var additionalTotal = 0d;
        foreach (var item in additional)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || item.Amount < 0 || item.Quantity <= 0) errors["additionalItems"] = new[] { "Additional item name, amount, and quantity are invalid." };
            else additionalTotal += item.Amount * item.Quantity;
        }
        if (errors.Count > 0 || !validDimensions) throw new KitchenValidationException(errors);

        var total = Math.Round(Math.Round(widthMm / 304.8, 1) * Math.Round(heightMm / 304.8, 1) * Math.Round(depthMm / 304.8, 1) * materialRate + accessoryTotal + additionalTotal, 2);
        await using var transaction = await _context.Database.BeginTransactionAsync();
        order.Parent = parent; order.UtilityName = utility; order.UtilityNameOld = string.IsNullOrWhiteSpace(request.UtilityNameOld) ? "KitchenUtility" : request.UtilityNameOld.Trim();
        order.Width = request.Width!.Trim(); order.Height = request.Height!.Trim(); order.Depth = request.Depth!.Trim(); order.Materials = material;
        order.Accessories = string.Join(",", request.Accessories); order.Quantities = string.Join(",", request.Quantities);
        order.AdditionalItemName = string.Join(",", additional.Select(item => item.Name!.Trim()));
        order.AdditionalItemsAmounts = string.Join(",", additional.Select(item => item.Amount.ToString(CultureInfo.InvariantCulture)));
        order.AdditionalItemsQuantities = string.Join(",", additional.Select(item => item.Quantity.ToString(CultureInfo.InvariantCulture)));
        order.MaterialTotal = materialRate; order.AccessoriesTotal = accessoryTotal; order.AdditionalItemsTotal = additionalTotal; order.TotalPrice = total;

        var roomOrders = await _context.OrderDetails.Where(item => item.ProjectVersionDetailsId == projectId && item.ProjectId == projectId && (item.UtilityName == utility || item.UtilityNameOld == utility)).ToListAsync();
        var roomDetails = await _context.ProjectDetails.Where(item => item.ProjectId == projectId && item.RoomName == utility).ToListAsync();
        var woodwork = roomOrders.Sum(item => item.MaterialTotal); var accessories = roomOrders.Sum(item => item.AccessoriesTotal); var services = roomOrders.Sum(item => item.AdditionalItemsTotal); var roomTotal = roomOrders.Sum(item => item.TotalPrice);
        if (roomDetails.Count > 0)
        {
            roomDetails[0].Woodwork = woodwork.ToString(CultureInfo.InvariantCulture); roomDetails[0].Accessories = accessories.ToString(CultureInfo.InvariantCulture); roomDetails[0].Services = services.ToString(CultureInfo.InvariantCulture); roomDetails[0].Total = roomTotal.ToString(CultureInfo.InvariantCulture);
            foreach (var detail in roomDetails.Skip(1)) { detail.Woodwork = "0"; detail.Accessories = "0"; detail.Services = "0"; detail.Total = "0"; }
        }
        var allDetails = await _context.ProjectDetails.Where(item => item.ProjectId == projectId).ToListAsync();
        var grandTotal = allDetails.Sum(item => double.TryParse(item.Total, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0d);
        var discountAmount = Math.Min(Math.Max(project.DiscountAmount, 0d), grandTotal);
        project.GrandTotal = grandTotal;
        project.DiscountAmount = discountAmount;
        project.DiscountedTotal = grandTotal - discountAmount;
        await _context.SaveChangesAsync(); await transaction.CommitAsync();
        return new KitchenItemResponse { Id = order.OrderId, ProjectId = projectId, Parent = order.Parent, UtilityName = order.UtilityName, UtilityNameOld = order.UtilityNameOld, Width = order.Width, Height = order.Height, Depth = order.Depth, Materials = order.Materials, Accessories = order.Accessories, Quantities = order.Quantities, AdditionalItems = order.AdditionalItemName, MaterialTotal = order.MaterialTotal, AccessoriesTotal = order.AccessoriesTotal, AdditionalItemsTotal = order.AdditionalItemsTotal, TotalPrice = order.TotalPrice, CreatedAt = order.OrderDate };
    }

    private static bool TryDimension(string? value,string key,Dictionary<string, string[]> errors,out double result)
    {
        if (!double.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out result) || result <= 0)
        {
            errors[key] = new[]
            {
                "Dimension must be a valid positive number in millimetres."
            };

            return false;
        }

        return true;
    }
}

public sealed class KitchenValidationException : Exception
{
    public Dictionary<string, string[]> Errors { get; }

    public KitchenValidationException(Dictionary<string, string[]> errors) : base("Kitchen input validation failed.")
    {
        Errors = errors;
    }
}
