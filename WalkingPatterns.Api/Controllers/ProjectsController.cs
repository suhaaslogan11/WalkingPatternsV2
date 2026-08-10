using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;

namespace WalkingPatterns.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;

        public ProjectsController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        [HttpGet("clients/{clientId:int}/projects")]
        public async Task<IActionResult> GetProjects(int clientId)
        {
            if (!await _projectService.ClientExistsAsync(clientId))
                return NotFound(new { message = "Client not found." });

            var projects = await _projectService.GetProjectsByClientIdAsync(clientId);
            return Ok(projects);
        }

        [HttpGet("projects/{id:int}")]
        public async Task<IActionResult> GetProject(int id)
        {
            var project = await _projectService.GetProjectByIdAsync(id);

            if (project == null)
                return NotFound(new { message = "Project not found." });

            return Ok(project);
        }

        [HttpGet("projects/{projectId:int}/details")]
        public async Task<IActionResult> GetProjectDetails(int projectId)
        {
            var details = await _projectService.GetProjectDetailPageAsync(projectId);

            if (details == null)
                return NotFound(new { message = "Project not found." });

            return Ok(details);
        }

        [HttpGet("projects/{projectId:int}/cart")]
        public async Task<IActionResult> GetProjectCart(int projectId)
        {
            var cart = await _projectService.GetProjectCartAsync(projectId);

            if (cart == null)
                return NotFound(new { message = "Project not found." });

            return Ok(cart);
        }

        [HttpDelete("projects/{projectId:int}/cart/{source}/{itemId:int}")]
        public async Task<IActionResult> DeleteProjectCartItem(int projectId, string source, int itemId)
        {
            if (source is not ("Kitchen" or "Bedroom" or "OtherWoodwork" or "HDS"))
                return BadRequest(new { message = "Invalid cart item source." });

            var deleted = await _projectService.DeleteProjectCartItemAsync(projectId, source, itemId);

            if (!deleted)
                return NotFound(new { message = "Cart item not found." });

            return NoContent();
        }

        [HttpGet("project-details/{projectDetailId:int}/orders")]
        public async Task<IActionResult> GetProjectDetailOrders(int projectDetailId)
        {
            var orders = await _projectService.GetOrdersByProjectDetailIdAsync(projectDetailId);

            if (orders == null)
                return NotFound(new { message = "Project details not found." });

            return Ok(orders);
        }

        [HttpGet("projects/{projectId:int}/grand-total")]
        public async Task<IActionResult> GetGrandTotal(int projectId)
        {
            var financials = await _projectService.GetProjectFinancialsAsync(projectId);

            if (financials == null)
                return NotFound(new { message = "Project not found." });

            return Ok(financials);
        }

        [HttpPost("projects/{projectId:int}/discount")]
        public async Task<IActionResult> ApplyDiscount(
            int projectId,
            ApplyDiscountRequest request)
        {
            try
            {
                var financials = await _projectService.ApplyDiscountAsync(
                    projectId,
                    request.DiscountAmount);

                if (financials == null)
                    return NotFound(new { message = "Project not found." });

                return Ok(financials);
            }
            catch (ArgumentOutOfRangeException)
            {
                return BadRequest(new { message = "Invalid discount amount." });
            }
        }

        [HttpDelete("orders/{orderId:int}")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            var deleted = await _projectService.DeleteOrderAsync(orderId);

            if (!deleted)
                return NotFound(new { message = "Order not found." });

            return NoContent();
        }

        [HttpDelete("projects/{projectId:int}/modules/{projectDetailId:int}")]
        public async Task<IActionResult> DeleteProjectModule(int projectId, int projectDetailId)
        {
            var deleted = await _projectService.DeleteProjectModuleAsync(
                projectId,
                projectDetailId);

            if (!deleted)
                return NotFound(new { message = "Project module not found." });

            return NoContent();
        }

        [HttpPost("clients/{clientId:int}/projects")]
        public async Task<IActionResult> AddProject(int clientId, AddProjectRequest request)
        {
            if (!await _projectService.ClientExistsAsync(clientId))
                return NotFound(new { message = "Client not found." });

            if (await _projectService.ProjectNameExistsAsync(request.ProjectName))
                return BadRequest(new { message = "Project name already exists." });

            if (!DateTime.TryParseExact(
                    request.projectDate,
                    "dd-MM-yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedProjectDate))
            {
                ModelState.AddModelError(
                    nameof(request.projectDate),
                    "Project date must be in DD-MM-YYYY format.");
                return ValidationProblem(ModelState);
            }

            var project = await _projectService.AddProjectAsync(
                clientId,
                request,
                parsedProjectDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            return CreatedAtAction(nameof(GetProjects), new { clientId }, project);
        }

        [HttpDelete("projects/{id:int}")]
        public async Task<IActionResult> DeleteProject(int id)
        {
            var deleted = await _projectService.DeleteProjectAsync(id);

            if (!deleted)
                return NotFound(new { message = "Project not found." });

            return NoContent();
        }

        [HttpPut("projects/{id:int}")]
        public async Task<IActionResult> UpdateProject(int id, AddProjectRequest request)
        {
            if (await _projectService.ProjectNameExistsAsync(request.ProjectName, id))
                return BadRequest(new { message = "Project name already exists." });

            if (!DateTime.TryParseExact(
                    request.projectDate,
                    "dd-MM-yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedProjectDate))
            {
                ModelState.AddModelError(
                    nameof(request.projectDate),
                    "Project date must be in DD-MM-YYYY format.");
                return ValidationProblem(ModelState);
            }

            var project = await _projectService.UpdateProjectAsync(
                id,
                request,
                parsedProjectDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));

            if (project == null)
                return NotFound(new { message = "Project not found." });

            return Ok(project);
        }
    }
}
