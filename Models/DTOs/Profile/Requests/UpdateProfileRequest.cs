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

        [Required(ErrorMessage = "Địa chỉ thường trú không được để trống")]
        [MinLength(10, ErrorMessage = "Địa chỉ thường trú phải có ít nhất 10 ký tự")]
        [DefaultValue("54 Nguyễn Lương Bằng, Phường Hoà Khánh Bắc, Quận Liên Chiểu, TP. Đà Nẵng")]
        public string PermanentAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên thân nhân không được để trống")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ tên thân nhân chỉ được chứa chữ cái và khoảng trắng")]
        [DefaultValue("Nguyễn Văn Tuấn")]
        public string RelativeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại thân nhân không được để trống")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại thân nhân phải bắt đầu bằng đầu số hợp lệ và có đúng 10 chữ số")]
        [DefaultValue("0941809193")]
        public string RelativePhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mối quan hệ không được để trống")]
        [DefaultValue("Bố")]
        public string Relationship { get; set; } = string.Empty;
    }
}