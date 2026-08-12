using Microsoft.AspNetCore.Mvc;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Services;

namespace WalkingPatterns.Api.Controllers;

[ApiController]
[Route("api")]
public class KitchenController : ControllerBase
{
    private readonly IKitchenPricingService _service;
    public KitchenController(IKitchenPricingService service) => _service = service;

    [HttpGet("kitchen/pricing")]
    public ActionResult<KitchenPricingResponse> GetPricing() => Ok(_service.GetPricing());

    [HttpPost("projects/{projectId:int}/kitchen-items")]
    public async Task<IActionResult> CalculateAndSave(int projectId, KitchenItemRequest request)
    {
        try
        {
            var result = await _service.CalculateAndSaveAsync(projectId, request);
            return result == null ? NotFound(new { message = "Project not found." }) : Ok(result);
        }
        catch (KitchenValidationException exception)
        {
            return BadRequest(new { message = exception.Message, errors = exception.Errors });
        }
    }
}
