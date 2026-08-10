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
