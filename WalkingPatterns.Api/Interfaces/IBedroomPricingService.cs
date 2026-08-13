using WalkingPatterns.Api.DTOs;

namespace WalkingPatterns.Api.Interfaces;

public interface IBedroomPricingService
{
    BedroomPricingResponse GetPricing();
    Task<BedroomItemResponse?> CalculateAndSaveAsync(int projectId, BedroomItemRequest request);
}
