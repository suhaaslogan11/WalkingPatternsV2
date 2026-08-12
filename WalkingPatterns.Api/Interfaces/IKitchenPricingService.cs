using WalkingPatterns.Api.DTOs;

namespace WalkingPatterns.Api.Interfaces;

public interface IKitchenPricingService
{
    KitchenPricingResponse GetPricing();
    Task<KitchenItemResponse?> CalculateAndSaveAsync(int projectId, KitchenItemRequest request);
}
