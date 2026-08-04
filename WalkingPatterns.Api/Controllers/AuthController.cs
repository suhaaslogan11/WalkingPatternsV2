using Microsoft.AspNetCore.Mvc;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;

namespace WalkingPatterns.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);

            if (!result)
            {
                return BadRequest(new
                {
                    Message = "User already exists."
                });
            }

            return Ok(new
            {
                Message = "User registered successfully."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var token = await _authService.LoginAsync(request);

            if (token == null)
            {
                return Unauthorized(new
                {
                    Message = "Invalid email or password."
                });
            }

            return Ok(new
            {
                Token = token
            });
        }
    }
}