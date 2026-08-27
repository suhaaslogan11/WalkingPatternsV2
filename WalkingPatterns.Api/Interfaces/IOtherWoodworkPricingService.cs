using WalkingPatterns.Api.DTOs;

namespace WalkingPatterns.Api.Interfaces;

public interface IOtherWoodworkPricingService
{
    OtherWoodworkPricingResponse GetPricing();
    Task<OtherWoodworkItemResponse?> CalculateAndSaveAsync(int projectId, OtherWoodworkItemRequest request);
    Task<OtherWoodworkItemResponse?> UpdateOrderAsync(int projectId, int orderId, OtherWoodworkItemRequest request);
}
