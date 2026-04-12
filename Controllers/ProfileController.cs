using System.Security.Claims;
using BackendAPI.Models.DTOs.Profile.Requests;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController(IProfileService _profileService) : ControllerBase
    {
        private int GetUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr))
            {
                throw new UnauthorizedAccessException("Ng??i dùng ch?a ??ng nh?p");
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
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var (success, message) = await _profileService.UpdateProfileAsync(userId, request);
            
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var (success, message) = await _profileService.ChangePasswordAsync(userId, request);
            
            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}
