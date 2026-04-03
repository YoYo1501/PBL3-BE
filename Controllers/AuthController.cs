using BackendAPI.Models.DTOs.Auth.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {

        var result = await _authService.LoginAsync(dto);

        if (result == null)
            return Unauthorized("Email hoặc mật khẩu không đúng");

        return Ok(result);
    }
}