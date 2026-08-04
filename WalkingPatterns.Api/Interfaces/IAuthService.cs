using WalkingPatterns.Api.DTOs;

namespace WalkingPatterns.Api.Interfaces;

public interface IAuthService
{
    Task<bool> RegisterAsync(RegisterRequest request);

    Task<string?> LoginAsync(LoginRequest request);
}