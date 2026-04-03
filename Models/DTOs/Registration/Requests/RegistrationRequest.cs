using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
namespace BackendAPI.Models.DTOs.Registration.Requests
{
    public class RegistrationRequestDto: IValidatableObject
    {
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(100)]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng")]
        [DefaultValue("Trần Văn B")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Căn cước công dân không được để trống")]
        [RegularExpression(@"^\d{12}$", ErrorMessage = "Căn cước công dân phải bao gồm đúng 12 chữ số")]
        [DefaultValue("048201002345")]
        public string CitizenId { get; set; } = string.Empty;

        [Required(ErrorMessage = "Giới tính không được để trống")]
        [RegularExpression(@"^(Nam|Nữ)$", ErrorMessage = "Giới tính phải là Nam hoặc Nữ")]
        [DefaultValue("Nam")]
        public string Gender { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng đầu số hợp lệ và có đúng 10 chữ số")]
        [DefaultValue("0987654321")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [DefaultValue("studentb@gmail.com")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ thường trú không được để trống")]
        [MinLength(10, ErrorMessage = "Địa chỉ thường trú phải có ít nhất 10 ký tự")]
        [DefaultValue("456 Đường XYZ, Hà Nội")]
        public string PermanentAddress { get; set; } = string.Empty;

        // Thân nhân
        [Required(ErrorMessage = "Họ tên thân nhân không được để trống")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ tên thân nhân chỉ được chứa chữ cái và khoảng trắng")]
        [DefaultValue("Trần Văn C")]
        public string RelativeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại thân nhân không được để trống")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại thân nhân không hợp lệ")]
        [DefaultValue("0999888777")]
        public string RelativePhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mối quan hệ không được để trống")]
        [DefaultValue("Phụ huynh")]
        public string Relationship { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã phòng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã phòng không hợp lệ")]
        [DefaultValue(1)]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime EndDate { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "Ngày kết thúc phải lớn hơn ngày bắt đầu",
                    new[] { nameof(EndDate) }
                );
            }

            if (StartDate.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Ngày bắt đầu không được ở quá khứ",
                    new[] { nameof(StartDate) }
                );
            }
        }
    }
}