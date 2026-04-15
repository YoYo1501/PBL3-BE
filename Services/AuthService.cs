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

public class AuthService(
    IAuthRepository _authRepository,
    IConfiguration _config) : IAuthService
{

    public async Task<(LoginResponse? Data, string? Error)> LoginAsync(LoginRequest dto)
    {
        var citizenId = dto.CitizenId.Trim();
        var user = await _authRepository.GetUserByCitizenIdAsync(citizenId);


        if (user == null)
            return (null, "Tên đăng nhập hoặc mật khẩu không đúng");

        if (!user.IsActive)
            return (null, "Tài khoản đã bị khóa");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return (null, "Tên đăng nhập hoặc mật khẩu không đúng");

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
        // 1. Lấy đúng Key từ file Json
        var jwtKey = _config["Jwt:Key"] ?? "supersecretkey_dormitory_2026_abcxyz_security_key";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        // 2. Thiết lập Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.CitizenId),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Role, user.Role) // Đảm bảo user.Role là "Admin"
        };
        
        if (user.Student != null)
        {
            claims.Add(new Claim("StudentId", user.Student.Id.ToString()));
        }

        // 3. Tạo Token
        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "BackendAPI",
            audience: _config["Jwt:Audience"] ?? "FrontendApp",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpireMinutes"] ?? "120")),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}