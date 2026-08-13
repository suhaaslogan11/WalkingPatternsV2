using Microsoft.AspNetCore.Mvc;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Services;
namespace WalkingPatterns.Api.Controllers;
[ApiController, Route("api")]
public class HdsController : ControllerBase
{
    private readonly IHdsPricingService _service; public HdsController(IHdsPricingService service) => _service = service;
    [HttpGet("hds/pricing")] public ActionResult<HdsPricingResponse> GetPricing() => Ok(_service.GetPricing());
    [HttpPost("projects/{projectId:int}/hds-items")]
    public async Task<IActionResult> CalculateAndSave(int projectId, HdsItemRequest request) { try { var result = await _service.CalculateAndSaveAsync(projectId, request); return result == null ? NotFound(new { message = "Project not found." }) : Ok(result); } catch (HdsValidationException exception) { return BadRequest(new { message = exception.Message, errors = exception.Errors }); } }
}
