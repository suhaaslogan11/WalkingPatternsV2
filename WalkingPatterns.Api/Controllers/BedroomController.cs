using Microsoft.AspNetCore.Mvc;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Services;

namespace WalkingPatterns.Api.Controllers;

[ApiController]
[Route("api")]
public class BedroomController : ControllerBase
{
    private readonly IBedroomPricingService _service;
    public BedroomController(IBedroomPricingService service) => _service = service;

    [HttpGet("bedroom/pricing")]
    public ActionResult<BedroomPricingResponse> GetPricing() => Ok(_service.GetPricing());

    [HttpPost("projects/{projectId:int}/bedroom-items")]
    public async Task<IActionResult> CalculateAndSave(int projectId, BedroomItemRequest request)
    {
        try
        {
            var result = await _service.CalculateAndSaveAsync(projectId, request);
            return result == null ? NotFound(new { message = "Project not found." }) : Ok(result);
        }
        catch (BedroomValidationException exception)
        {
            return BadRequest(new { message = exception.Message, errors = exception.Errors });
        }
    }
}
