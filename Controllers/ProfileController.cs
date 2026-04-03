using System.Security.Claims;
using BackendAPI.Models.DTOs.Profile.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize] // T?m th?i comment l?i ?? test Swagger
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        // T?m th?i fix c?ng `userId` là 2 (Sinh viên m?u Nguy?n V?n A) ?? d? test trên Swagger
        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                return 2; // Gi? s? 2 là ID c?a sinh viên ?? test n?u ch?a ??ng nh?p
            }
            return int.Parse(userIdStr);
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var (success, message, data) = await _profileService.GetProfileAsync(userId);
            
            if (!success)
                return NotFound(new { message });
            
            return Ok(new { message, data });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = GetUserId();
            var (success, message) = await _profileService.UpdateProfileAsync(userId, request);
            
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = GetUserId();
            var (success, message) = await _profileService.ChangePasswordAsync(userId, request);
            
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}
