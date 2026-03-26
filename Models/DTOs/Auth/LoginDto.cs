using System.ComponentModel;

namespace BackendAPI.Models.DTOs.Auth
{
    public class LoginDto
    {
        [DefaultValue("admin@ktx.edu.vn")]
        public string Email { get; set; } = string.Empty;

        [DefaultValue("Admin@123")]
        public string Password { get; set; } = string.Empty;
    }
}
