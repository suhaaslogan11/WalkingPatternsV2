using WalkingPatterns.Api.DTOs;

namespace WalkingPatterns.Api.Interfaces
{
    public interface IProjectService
    {
        Task<bool> ClientExistsAsync(int clientId);
        Task<bool> ProjectNameExistsAsync(string projectName, int? excludeProjectId = null);
        Task<List<ProjectResponse>> GetProjectsByClientIdAsync(int clientId);
        Task<ProjectResponse?> GetProjectByIdAsync(int id);
        Task<ProjectDetailPageResponse?> GetProjectDetailPageAsync(int projectId);
        Task<ProjectOrdersResponse?> GetOrdersByProjectDetailIdAsync(int projectDetailId);
        Task<ProjectFinancialResponse?> GetProjectFinancialsAsync(int projectId);
        Task<ProjectFinancialResponse?> ApplyDiscountAsync(int projectId, double discountAmount);
        Task<bool> DeleteOrderAsync(int orderId);
        Task<bool> DeleteProjectModuleAsync(int projectId, int projectDetailId);
        Task<ProjectResponse> AddProjectAsync(int clientId, AddProjectRequest request, string storedProjectDate);
        Task<ProjectResponse?> UpdateProjectAsync(int id, AddProjectRequest request, string storedProjectDate);
        Task<bool> DeleteProjectAsync(int id);
    }
}
