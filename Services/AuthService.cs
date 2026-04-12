using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendAPI.Data;
using BackendAPI.Models.DTOs.Auth.Requests;
using BackendAPI.Models.DTOs.Auth.Responses;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace BackendAPI.Services;

public class AuthService : IAuthService
{
    private readonly IAuthRepository _authRepository;
    private readonly IConfiguration _config;

    public AuthService(
        IAuthRepository authRepository,
        IConfiguration config
    )
    {
        _authRepository = authRepository;
        _config = config;
    }

    public async Task<(LoginResponse? Data, string? Error)> LoginAsync(LoginRequest dto)
    {
        var email = dto.Email.Trim().ToLower();
        var user = await _authRepository.GetUserByEmailAsync(email);


        if (user == null)
            return (null, "Email hoặc mật khẩu không đúng");

        if (!user.IsActive)
            return (null, "Tài khoản đã bị khóa");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return (null, "Email hoặc mật khẩu không đúng");

        var token = GenerateToken(user);

        return (new LoginResponse
        {
            Token = token,
            Role = user.Role,
            FullName = user.Student?.FullName ?? "",
            UserId = user.Id
        }, null);
    }



    private string GenerateToken(BackendAPI.Models.Entities.User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                double.Parse(_config["Jwt:ExpireMinutes"]!)),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}