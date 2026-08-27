using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using WalkingPatterns.Api.Data;
using WalkingPatterns.Api.DTOs;
using WalkingPatterns.Api.Interfaces;
using WalkingPatterns.Api.Models;

namespace WalkingPatterns.Api.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly JwtService _jwtService;

    public AuthService(AppDbContext context,
                       JwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(x => x.Email == request.Email))
            return false;

        var user = new User
        {
            UserName = request.UserName.Trim(),
            FullName = request.FullName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = "Admin"
        };

        _context.Users.Add(user);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<string?> LoginAsync(LoginRequest request)
    {
        var login = request.UsernameOrEmail.Trim();

        var user = await _context.Users
            .FirstOrDefaultAsync(x =>
                x.Email == login ||
                x.UserName == login);

        if (user == null)
            return null;

        var valid = BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash);

        if (!valid)
            return null;

        return _jwtService.GenerateToken(user);
    }
}