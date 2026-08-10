using System.Globalization;
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
