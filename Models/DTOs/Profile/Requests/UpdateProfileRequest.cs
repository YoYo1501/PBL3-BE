using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.DTOs.Profile.Requests
{
    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng đầu số hợp lệ (03, 05, 07, 08, 09) và có đúng 10 chữ số")]
        [DefaultValue("0582207461")]
        public string Phone { get; set; } = string.Empty;

        public string? PermanentAddress { get; set; }
        
        [RegularExpression(@"^[\p{L}\s]*$", ErrorMessage = "Họ tên thân nhân chỉ được chứa chữ cái và khoảng trắng")]
        public string? RelativeName { get; set; }

        [RegularExpression(@"^(?:(03|05|07|08|09)\d{8})?$", ErrorMessage = "Số điện thoại thân nhân phải bắt đầu bằng đầu số hợp lệ và có đúng 10 chữ số")]
        public string? RelativePhone { get; set; }

        public string? Relationship { get; set; }
    }
}