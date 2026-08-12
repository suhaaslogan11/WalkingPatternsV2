using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using WalkingPatterns.Api.Data;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Models;

namespace WalkingPatterns.Api.Services
{
    public class ProjectService : IProjectService
    {
        private readonly AppDbContext _context;

        public ProjectService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ClientExistsAsync(int clientId)
        {
            return await _context.Clients.AnyAsync(client => client.ClientId == clientId);
        }

        public async Task<bool> ProjectNameExistsAsync(string projectName, int? excludeProjectId = null)
        {
            var normalizedProjectName = projectName.ToUpper();

            return await _context.ProjectVersionDetails
                .AnyAsync(project =>
                    project.ProjectName.ToUpper() == normalizedProjectName &&
                    (!excludeProjectId.HasValue || project.Id != excludeProjectId.Value));
        }

        public async Task<List<ProjectResponse>> GetProjectsByClientIdAsync(int clientId)
        {
            var projects = await _context.ProjectVersionDetails
                .Where(project => project.ClientId == clientId)
                .OrderBy(project => project.Id)
                .ToListAsync();

            return projects.Select(MapToResponse).ToList();
        }

        public async Task<ProjectResponse?> GetProjectByIdAsync(int id)
        {
            var project = await _context.ProjectVersionDetails.FindAsync(id);

            return project == null ? null : MapToResponse(project);
        }

        public async Task<ProjectDetailPageResponse?> GetProjectDetailPageAsync(int projectId)
        {
            var project = await _context.ProjectVersionDetails
                .Include(item => item.ProjectDetails)
                .SingleOrDefaultAsync(item => item.Id == projectId);

            if (project == null)
                return null;

            var modules = project.ProjectDetails
                .GroupBy(detail => detail.RoomName)
                .Select(group =>
                {
                    var accessories = group.Sum(detail => ParseCommaSeparatedTotal(detail.Accessories));
                    var services = group.Sum(detail => ParseCommaSeparatedTotal(detail.Services));
                    var total = group.Sum(detail => ParseCommaSeparatedTotal(detail.Total));

                    return new ModuleSummaryResponse
                    {
                        ProjectDetailId = group.First().Id,
                        RoomName = group.Key,
                        Accessories = accessories,
                        Services = services,
                        Total = total,
                        Woodwork = total - accessories - services
                    };
                })
                .ToList();

            return new ProjectDetailPageResponse
            {
                ProjectId = project.Id,
                ClientName = project.ClientName,
                ProjectName = project.ProjectName,
                projectDate = FormatProjectDate(project.projectDate),
                VersionNumber = project.VersionNumber,
                Modules = modules
            };
        }

        public async Task<ProjectOrdersResponse?> GetOrdersByProjectDetailIdAsync(int projectDetailId)
        {
            var projectDetail = await _context.ProjectDetails
                .AsNoTracking()
                .SingleOrDefaultAsync(detail => detail.Id == projectDetailId);

            if (projectDetail == null)
                return null;

            var orderEntities = await _context.OrderDetails
                .AsNoTracking()
                .Where(order =>
                    order.ProjectVersionDetailsId == projectDetail.ProjectId &&
                    order.ProjectId == projectDetail.ProjectId &&
                    order.UtilityName == projectDetail.RoomName)
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            var orders = orderEntities.Select(order => new OrderDetailResponse
                {
                    OrderId = order.OrderId,
                    Parent = order.Parent,
                    Materials = order.Materials,
                    Width = order.Width,
                    Height = order.Height,
                    Depth = order.Depth,
                    Accessories = order.Accessories,
                    Quantities = order.Quantities,
                    AdditionalItemName = order.AdditionalItemName,
                    AdditionalItemsAmounts = order.AdditionalItemsAmounts,
                    AdditionalItemsQuantities = order.AdditionalItemsQuantities,
                    MaterialTotal = order.MaterialTotal,
                    AccessoriesTotal = order.AccessoriesTotal,
                    AdditionalItemsTotal = order.AdditionalItemsTotal,
                    TotalPrice = order.TotalPrice,
                    UtilityNameOld = order.UtilityNameOld,
                    OrderDate = order.OrderDate.ToString("yyyy-MM-dd")
                })
                .ToList();

            return new ProjectOrdersResponse
            {
                ProjectDetailId = projectDetail.Id,
                RoomName = projectDetail.RoomName,
                Orders = orders
            };
        }

        public async Task<ProjectFinancialResponse?> GetProjectFinancialsAsync(int projectId)
        {
            var project = await _context.ProjectVersionDetails.FindAsync(projectId);

            if (project == null)
                return null;

            var grandTotal = await _context.OrderDetails
                .Where(order => order.ProjectVersionDetailsId == projectId)
                .SumAsync(order => order.TotalPrice);

            return new ProjectFinancialResponse
            {
                GrandTotal = grandTotal,
                DiscountAmount = project.DiscountAmount,
                DiscountedTotal = grandTotal - project.DiscountAmount
            };
        }

        public async Task<ProjectFinancialResponse?> ApplyDiscountAsync(int projectId, double discountAmount)
        {
            var project = await _context.ProjectVersionDetails.FindAsync(projectId);

            if (project == null)
                return null;

            var grandTotal = await _context.OrderDetails
                .Where(order => order.ProjectVersionDetailsId == projectId)
                .SumAsync(order => order.TotalPrice);

            if (discountAmount < 0 || discountAmount > grandTotal)
                throw new ArgumentOutOfRangeException(nameof(discountAmount), "Invalid discount amount.");

            project.GrandTotal = grandTotal;
            project.DiscountAmount = discountAmount;
            project.DiscountedTotal = grandTotal - discountAmount;

            await _context.SaveChangesAsync();

            return new ProjectFinancialResponse
            {
                GrandTotal = project.GrandTotal,
                DiscountAmount = project.DiscountAmount,
                DiscountedTotal = project.DiscountedTotal
            };
        }

        public async Task<bool> DeleteOrderAsync(int orderId)
        {
            var order = await _context.OrderDetails.FindAsync(orderId);

            if (order == null)
                return false;

            _context.OrderDetails.Remove(order);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteProjectModuleAsync(int projectId, int projectDetailId)
        {
            var selectedDetail = await _context.ProjectDetails
                .SingleOrDefaultAsync(detail =>
                    detail.Id == projectDetailId &&
                    detail.ProjectId == projectId);

            if (selectedDetail == null)
                return false;

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var roomName = selectedDetail.RoomName;
            var projectDetails = await _context.ProjectDetails
                .Where(detail =>
                    detail.ProjectId == projectId &&
                    detail.RoomName == roomName)
                .ToListAsync();

            var relatedOrders = await _context.OrderDetails
                .Where(order =>
                    order.ProjectVersionDetailsId == projectId &&
                    order.ProjectId == projectId &&
                    order.UtilityName == roomName)
                .ToListAsync();

            _context.OrderDetails.RemoveRange(relatedOrders);
            _context.ProjectDetails.RemoveRange(projectDetails);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }

        public async Task<List<ProjectCartItemResponse>?> GetProjectCartAsync(int projectId)
        {
            var projectName = await _context.ProjectVersionDetails
                .Where(project => project.Id == projectId)
                .Select(project => project.ProjectName)
                .SingleOrDefaultAsync();

            if (projectName == null)
                return null;

            var items = new List<ProjectCartItemResponse>();

            var kitchen = await _context.KitchenPriceDetails.AsNoTracking()
                .Where(item => item.ProjectName == projectName)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync();
            items.AddRange(kitchen.Select(item => new ProjectCartItemResponse
            {
                Id = item.Id, Source = "Kitchen", Parent = item.Parent,
                UtilityName = item.UtilityName, UtilityNameOld = item.UtilityNameOld,
                ProjectName = item.ProjectName, Width = item.Width, Height = item.Height, Depth = item.Depth,
                Materials = item.Materials, Accessories = item.Accessories, Quantities = item.Quantities,
                AdditionalItemName = item.AdditionalItemName, AdditionalItemsAmounts = item.AdditionalItemsAmounts,
                AdditionalItemsQuantities = item.AdditionalItemsQuantities,
                MaterialTotal = item.MaterialTotal ?? 0, AccessoriesTotal = item.AccessoriesTotal ?? 0,
                AdditionalItemsTotal = item.AdditionalItemsTotal ?? 0, TotalPrice = item.TotalPrice ?? 0,
                CreatedAt = item.CreatedAt
            }));

            var bedroom = await _context.BedromPriceDetails.AsNoTracking()
                .Where(item => item.ProjectName == projectName)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync();
            items.AddRange(bedroom.Select(item => MapCartItem(item.Id, "Bedroom", item.Parent, item.UtilityName,
                item.UtilityNameOld, item.ProjectName, item.Width, item.Height, item.Depth, item.Materials, null, null,
                item.AdditionalItemName, item.AdditionalItemsAmounts, item.AdditionalItemsQuantities,
                item.MaterialTotal, 0, item.AdditionalItemsTotal, item.TotalPrice, item.CreatedAt)));

            var other = await _context.OtherWoodworkPriceDetails.AsNoTracking()
                .Where(item => item.ProjectName == projectName)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync();
            items.AddRange(other.Select(item => MapCartItem(item.Id, "OtherWoodwork", item.Parent, item.UtilityName,
                item.UtilityNameOld, item.ProjectName, item.Width, item.Height, item.Depth, item.Materials, null, null,
                item.AdditionalItemName, item.AdditionalItemsAmounts, item.AdditionalItemsQuantities,
                item.MaterialTotal, 0, item.AdditionalItemsTotal, item.TotalPrice, item.CreatedAt)));

            var hds = await _context.HDSPriceDetails.AsNoTracking()
                .Where(item => item.ProjectName == projectName)
                .OrderByDescending(item => item.CreatedAt)
                .ToListAsync();
            items.AddRange(hds.Select(item => MapCartItem(item.Id, "HDS", item.Parent, item.UtilityName,
                item.UtilityNameOld, item.ProjectName, item.Width, item.Height, item.Depth, item.Materials, null, null,
                item.AdditionalItemName, item.AdditionalItemsAmounts, item.AdditionalItemsQuantities,
                item.MaterialTotal, 0, item.AdditionalItemsTotal, item.TotalPrice, item.CreatedAt)));

            return items.OrderByDescending(item => item.CreatedAt).ToList();
        }

        public async Task<bool> DeleteProjectCartItemAsync(int projectId, string source, int itemId)
        {
            var projectName = await _context.ProjectVersionDetails
                .Where(project => project.Id == projectId)
                .Select(project => project.ProjectName)
                .SingleOrDefaultAsync();

            if (projectName == null)
                return false;

            switch (source)
            {
                case "Kitchen":
                    var kitchen = await _context.KitchenPriceDetails
                        .SingleOrDefaultAsync(item => item.Id == itemId && item.ProjectName == projectName);
                    if (kitchen == null) return false;
                    _context.KitchenPriceDetails.Remove(kitchen);
                    break;
                case "Bedroom":
                    var bedroom = await _context.BedromPriceDetails
                        .SingleOrDefaultAsync(item => item.Id == itemId && item.ProjectName == projectName);
                    if (bedroom == null) return false;
                    _context.BedromPriceDetails.Remove(bedroom);
                    break;
                case "OtherWoodwork":
                    var other = await _context.OtherWoodworkPriceDetails
                        .SingleOrDefaultAsync(item => item.Id == itemId && item.ProjectName == projectName);
                    if (other == null) return false;
                    _context.OtherWoodworkPriceDetails.Remove(other);
                    break;
                case "HDS":
                    var hds = await _context.HDSPriceDetails
                        .SingleOrDefaultAsync(item => item.Id == itemId && item.ProjectName == projectName);
                    if (hds == null) return false;
                    _context.HDSPriceDetails.Remove(hds);
                    break;
                default:
                    return false;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProjectCheckoutResponse?> CheckoutProjectAsync(int projectId)
        {
            var project = await _context.ProjectVersionDetails
                .SingleOrDefaultAsync(item => item.Id == projectId);

            if (project == null)
                return null;

            var kitchen = await _context.KitchenPriceDetails
                .Where(item => item.ProjectName == project.ProjectName)
                .ToListAsync();
            var bedroom = await _context.BedromPriceDetails
                .Where(item => item.ProjectName == project.ProjectName)
                .ToListAsync();
            var other = await _context.OtherWoodworkPriceDetails
                .Where(item => item.ProjectName == project.ProjectName)
                .ToListAsync();
            var hds = await _context.HDSPriceDetails
                .Where(item => item.ProjectName == project.ProjectName)
                .ToListAsync();

            var cartTotal = kitchen.Sum(item => item.TotalPrice ?? 0)
                + bedroom.Sum(item => item.TotalPrice)
                + other.Sum(item => item.TotalPrice)
                + hds.Sum(item => item.TotalPrice);
            var itemCount = kitchen.Count + bedroom.Count + other.Count + hds.Count;

            if (itemCount == 0)
                throw new InvalidOperationException("The project cart is empty.");

            var hasExistingCheckout = await _context.ProjectDetails
                .AnyAsync(detail => detail.ProjectId == projectId);

            await using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var item in kitchen)
            {
                AddCheckoutRows(
                    project,
                    item.Parent,
                    item.UtilityName,
                    item.UtilityNameOld,
                    item.Width,
                    item.Height,
                    item.Depth,
                    item.Materials,
                    item.Accessories,
                    item.Quantities,
                    item.AdditionalItemName,
                    item.AdditionalItemsAmounts,
                    item.AdditionalItemsQuantities,
                    item.MaterialTotal ?? 0,
                    item.AccessoriesTotal ?? 0,
                    item.AdditionalItemsTotal ?? 0,
                    item.TotalPrice ?? 0);
            }

            foreach (var item in bedroom)
                AddCheckoutRows(project, item.Parent, item.UtilityName, item.UtilityNameOld, item.Width, item.Height,
                    item.Depth, item.Materials, null, null, item.AdditionalItemName, item.AdditionalItemsAmounts,
                    item.AdditionalItemsQuantities, item.MaterialTotal, 0, item.AdditionalItemsTotal, item.TotalPrice);

            foreach (var item in other)
                AddCheckoutRows(project, item.Parent, item.UtilityName, item.UtilityNameOld, item.Width, item.Height,
                    item.Depth, item.Materials, null, null, item.AdditionalItemName, item.AdditionalItemsAmounts,
                    item.AdditionalItemsQuantities, item.MaterialTotal, 0, item.AdditionalItemsTotal, item.TotalPrice);

            foreach (var item in hds)
                AddCheckoutRows(project, item.Parent, item.UtilityName, item.UtilityNameOld, item.Width, item.Height,
                    item.Depth, item.Materials, null, null, item.AdditionalItemName, item.AdditionalItemsAmounts,
                    item.AdditionalItemsQuantities, item.MaterialTotal, 0, item.AdditionalItemsTotal, item.TotalPrice);

            _context.KitchenPriceDetails.RemoveRange(kitchen);
            _context.BedromPriceDetails.RemoveRange(bedroom);
            _context.OtherWoodworkPriceDetails.RemoveRange(other);
            _context.HDSPriceDetails.RemoveRange(hds);

            await _context.SaveChangesAsync();

            var grandTotal = await _context.OrderDetails
                .Where(order => order.ProjectVersionDetailsId == projectId)
                .SumAsync(order => order.TotalPrice);

            var discountAmount = project.DiscountAmount;
            if (double.IsNaN(discountAmount) || double.IsInfinity(discountAmount) || discountAmount < 0)
                discountAmount = 0;
            else if (discountAmount > grandTotal)
                discountAmount = grandTotal;
            project.GrandTotal = grandTotal;
            project.DiscountAmount = discountAmount;
            project.DiscountedTotal = grandTotal - discountAmount;
            project.VersionNumber = IncrementCheckoutVersion(project.VersionNumber, hasExistingCheckout);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new ProjectCheckoutResponse
            {
                CheckedOutItemCount = itemCount,
                CartTotal = cartTotal,
                GrandTotal = project.GrandTotal,
                DiscountAmount = project.DiscountAmount,
                DiscountedTotal = project.DiscountedTotal,
                VersionNumber = project.VersionNumber
            };
        }

        private void AddCheckoutRows(
            ProjectVersionDetails project, string? parent, string? utilityName, string? utilityNameOld,
            string? width, string? height, string? depth, string? materials, string? accessories,
            string? quantities, string? additionalItemName, string? additionalItemsAmounts,
            string? additionalItemsQuantities, double materialTotal, double accessoriesTotal,
            double additionalItemsTotal, double totalPrice)
        {
            var roomName = utilityName ?? utilityNameOld ?? string.Empty;
            _context.ProjectDetails.Add(new ProjectDetails
            {
                ProjectId = project.Id,
                RoomName = roomName,
                Woodwork = materialTotal.ToString(CultureInfo.InvariantCulture),
                Accessories = accessoriesTotal.ToString(CultureInfo.InvariantCulture),
                Services = additionalItemsTotal.ToString(CultureInfo.InvariantCulture),
                Total = totalPrice.ToString(CultureInfo.InvariantCulture),
                Width = width ?? string.Empty,
                Height = height ?? string.Empty,
                Depth = depth ?? string.Empty
            });

            _context.OrderDetails.Add(new OrderDetails
            {
                ProjectVersionDetailsId = project.Id,
                ProjectId = project.Id,
                ProjectName = project.ProjectName,
                Parent = parent,
                UtilityName = utilityName,
                UtilityNameOld = utilityNameOld,
                Materials = materials,
                Accessories = accessories,
                Quantities = quantities,
                AdditionalItemName = additionalItemName,
                AdditionalItemsAmounts = additionalItemsAmounts,
                AdditionalItemsQuantities = additionalItemsQuantities,
                MaterialTotal = materialTotal,
                AccessoriesTotal = accessoriesTotal,
                AdditionalItemsTotal = additionalItemsTotal,
                TotalPrice = totalPrice,
                Width = width,
                Height = height,
                Depth = depth,
                OrderDate = DateTime.UtcNow,
                GrandTotal = 0
            });
        }

        private static string IncrementCheckoutVersion(string? currentVersion, bool hasExistingDetails)
        {
            if (!hasExistingDetails)
                return "Version 1A";

            var match = Regex.Match(currentVersion ?? string.Empty, "^Version (\\d+)([A-Z])$");
            if (!match.Success)
                return "Version 1A";

            var number = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            var letter = match.Groups[2].Value[0];
            if (letter == 'Z')
            {
                number++;
                letter = 'A';
            }
            else
            {
                letter++;
            }

            return $"Version {number}{letter}";
        }

        private static ProjectCartItemResponse MapCartItem(
            int id, string source, string? parent, string? utilityName, string? utilityNameOld,
            string? projectName, string? width, string? height, string? depth, string? materials,
            string? accessories, string? quantities, string? additionalItemName, string? additionalItemsAmounts,
            string? additionalItemsQuantities, double materialTotal, double accessoriesTotal,
            double additionalItemsTotal, double totalPrice, DateTime createdAt)
        {
            return new ProjectCartItemResponse
            {
                Id = id, Source = source, Parent = parent, UtilityName = utilityName,
                UtilityNameOld = utilityNameOld, ProjectName = projectName, Width = width, Height = height,
                Depth = depth, Materials = materials, Accessories = accessories, Quantities = quantities,
                AdditionalItemName = additionalItemName, AdditionalItemsAmounts = additionalItemsAmounts,
                AdditionalItemsQuantities = additionalItemsQuantities, MaterialTotal = materialTotal,
                AccessoriesTotal = accessoriesTotal, AdditionalItemsTotal = additionalItemsTotal,
                TotalPrice = totalPrice, CreatedAt = createdAt
            };
        }

        public async Task<ProjectResponse> AddProjectAsync(
            int clientId,
            AddProjectRequest request,
            string storedProjectDate)
        {
            var client = await _context.Clients.FindAsync(clientId);

            if (client == null)
                throw new InvalidOperationException("Client not found.");

            var project = new ProjectVersionDetails
            {
                ProjectName = request.ProjectName,
                projectDate = storedProjectDate,
                VersionNumber = string.IsNullOrWhiteSpace(request.VersionNumber)
                    ? "Version 1A"
                    : request.VersionNumber,
                ClientId = client.ClientId,
                ClientName = client.ClientName
            };

            _context.ProjectVersionDetails.Add(project);
            await _context.SaveChangesAsync();

            return MapToResponse(project);
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.ProjectVersionDetails.FindAsync(id);

            if (project == null)
                return false;

            _context.ProjectVersionDetails.Remove(project);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ProjectResponse?> UpdateProjectAsync(
            int id,
            AddProjectRequest request,
            string storedProjectDate)
        {
            var project = await _context.ProjectVersionDetails.FindAsync(id);

            if (project == null)
                return null;

            project.ProjectName = request.ProjectName;
            project.projectDate = storedProjectDate;

            if (!string.IsNullOrWhiteSpace(request.VersionNumber))
                project.VersionNumber = request.VersionNumber;

            await _context.SaveChangesAsync();

            return MapToResponse(project);
        }

        private static ProjectResponse MapToResponse(ProjectVersionDetails project)
        {
            var projectDate = DateTime.ParseExact(
                project.projectDate,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture);

            return new ProjectResponse
            {
                Id = project.Id,
                ProjectName = project.ProjectName,
                projectDate = projectDate.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture),
                ClientId = project.ClientId,
                ClientName = project.ClientName,
                VersionNumber = project.VersionNumber,
                GrandTotal = project.GrandTotal,
                DiscountAmount = project.DiscountAmount,
                DiscountedTotal = project.DiscountedTotal
            };
        }

        private static decimal ParseCommaSeparatedTotal(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            return value
                .Split(',')
                .Sum(item => decimal.TryParse(
                    item.Trim(),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var parsed)
                    ? parsed
                    : 0);
        }

        private static string FormatProjectDate(string value)
        {
            var date = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            return date.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
        }
    }
}
