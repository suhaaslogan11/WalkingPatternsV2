using WalkingPatterns.Api.DTOs;
namespace WalkingPatterns.Api.Interfaces;
public interface IHdsPricingService { HdsPricingResponse GetPricing(); Task<HdsItemResponse?> CalculateAndSaveAsync(int projectId, HdsItemRequest request); Task<HdsItemResponse?> UpdateOrderAsync(int projectId, int orderId, HdsItemRequest request); }
