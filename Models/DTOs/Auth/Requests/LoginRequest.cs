using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.DTOs.Auth.Requests
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Tên đăng nhập (CCCD) phải bao gồm đúng 12 chữ số")]
        public string CitizenId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        public string Password { get; set; } = string.Empty;
    }
}
