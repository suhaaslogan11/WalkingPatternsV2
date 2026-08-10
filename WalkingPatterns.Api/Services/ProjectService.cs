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
    }
}
