using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.DTOs.Profile.Requests
{
    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "Mật khẩu cũ không được để trống")]
        [DefaultValue("048201002345")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mật khẩu mới không được để trống")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có ít nhất 8 ký tự")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Mật khẩu phải bao gồm ít nhất 1 chữ hoa, 1 chữ thường và 1 số")]
        [DefaultValue("NewPassword123")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp với mật khẩu mới")]
        [DefaultValue("NewPassword123")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}