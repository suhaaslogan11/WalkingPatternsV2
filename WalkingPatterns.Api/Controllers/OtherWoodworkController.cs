using Microsoft.AspNetCore.Mvc;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Services;

namespace WalkingPatterns.Api.Controllers;

[ApiController]
[Route("api")]
public class OtherWoodworkController : ControllerBase
{
    private readonly IOtherWoodworkPricingService _service;
    public OtherWoodworkController(IOtherWoodworkPricingService service) => _service = service;
    [HttpGet("other-woodwork/pricing")] public ActionResult<OtherWoodworkPricingResponse> GetPricing() => Ok(_service.GetPricing());
    [HttpPost("projects/{projectId:int}/other-woodwork-items")]
    public async Task<IActionResult> CalculateAndSave(int projectId, OtherWoodworkItemRequest request)
    {
        try { var result = await _service.CalculateAndSaveAsync(projectId, request); return result == null ? NotFound(new { message = "Project not found." }) : Ok(result); }
        catch (OtherWoodworkValidationException exception) { return BadRequest(new { message = exception.Message, errors = exception.Errors }); }
    }
}
