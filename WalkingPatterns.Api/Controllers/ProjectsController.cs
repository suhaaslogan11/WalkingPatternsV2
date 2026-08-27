using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Services;

namespace WalkingPatterns.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IKitchenPricingService _kitchenPricingService;
        private readonly IBedroomPricingService _bedroomPricingService;
        private readonly IOtherWoodworkPricingService _otherWoodworkPricingService;
        private readonly IHdsPricingService _hdsPricingService;

        public ProjectsController(IProjectService projectService, IKitchenPricingService kitchenPricingService, IBedroomPricingService bedroomPricingService, IOtherWoodworkPricingService otherWoodworkPricingService, IHdsPricingService hdsPricingService)
        {
            _projectService = projectService;
            _kitchenPricingService = kitchenPricingService;
            _bedroomPricingService = bedroomPricingService;
            _otherWoodworkPricingService = otherWoodworkPricingService;
            _hdsPricingService = hdsPricingService;
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

        [HttpPost("projects/{projectId:int}/checkout")]
        public async Task<IActionResult> CheckoutProject(int projectId)
        {
            try
            {
                var checkout = await _projectService.CheckoutProjectAsync(projectId);

                if (checkout == null)
                    return NotFound(new { message = "Project not found." });

                return Ok(checkout);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
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

        [HttpPut("projects/{projectId:int}/orders/{orderId:int}/kitchen")]
        public async Task<IActionResult> UpdateKitchenOrder(int projectId, int orderId, KitchenItemRequest request)
        {
            try
            {
                var updated = await _kitchenPricingService.UpdateOrderAsync(projectId, orderId, request);
                return updated == null ? NotFound(new { message = "Kitchen order not found for this project." }) : Ok(updated);
            }
            catch (KitchenValidationException exception)
            {
                return BadRequest(new { message = exception.Message, errors = exception.Errors });
            }
        }

        [HttpPut("projects/{projectId:int}/orders/{orderId:int}/bedroom")]
        public async Task<IActionResult> UpdateBedroomOrder(int projectId, int orderId, BedroomItemRequest request)
        {
            try { var result = await _bedroomPricingService.UpdateOrderAsync(projectId, orderId, request); return result == null ? NotFound(new { message = "Bedroom order not found for this project." }) : Ok(result); }
            catch (BedroomValidationException exception) { return BadRequest(new { message = exception.Message, errors = exception.Errors }); }
        }

        [HttpPut("projects/{projectId:int}/orders/{orderId:int}/other-woodwork")]
        public async Task<IActionResult> UpdateOtherWoodworkOrder(int projectId, int orderId, OtherWoodworkItemRequest request)
        {
            try { var result = await _otherWoodworkPricingService.UpdateOrderAsync(projectId, orderId, request); return result == null ? NotFound(new { message = "Other Woodwork order not found for this project." }) : Ok(result); }
            catch (OtherWoodworkValidationException exception) { return BadRequest(new { message = exception.Message, errors = exception.Errors }); }
        }

        [HttpPut("projects/{projectId:int}/orders/{orderId:int}/hds")]
        public async Task<IActionResult> UpdateHdsOrder(int projectId, int orderId, HdsItemRequest request)
        {
            try { var result = await _hdsPricingService.UpdateOrderAsync(projectId, orderId, request); return result == null ? NotFound(new { message = "HDS order not found for this project." }) : Ok(result); }
            catch (HdsValidationException exception) { return BadRequest(new { message = exception.Message, errors = exception.Errors }); }
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

        [HttpPut("projects/{projectId:int}/modules/{projectDetailId:int}/room-name")]
        public async Task<IActionResult> RenameProjectModule(
            int projectId,
            int projectDetailId,
            RenameProjectModuleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewRoomName))
                return BadRequest(new { message = "Room name is required." });

            var renamed = await _projectService.RenameProjectModuleAsync(
                projectId,
                projectDetailId,
                request.NewRoomName.Trim());

            if (!renamed)
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
