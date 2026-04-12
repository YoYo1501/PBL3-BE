using BackendAPI.Models.DTOs.Auth.Requests;
using BackendAPI.Models.Entities;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await authService.LoginAsync(dto);

        if (result.Data != null)
            return Ok(result.Data);

        return Unauthorized(new
        {
            message = result.Error
        });
    }
}