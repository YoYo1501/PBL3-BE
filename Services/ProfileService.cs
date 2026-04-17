using BackendAPI.Models.DTOs.Profile.Requests;
using BackendAPI.Models.DTOs.Profile.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services
{
    public class ProfileService(IProfileRepository _profileRepo) : IProfileService
    {

        public async Task<(bool Success, string Message, UserProfileResponse? Data)> GetProfileAsync(int userId)
        {
            var student = await _profileRepo.GetStudentByUserIdAsync(userId);

            if (student == null)
            {
                // Nếu User là Admin, có thể xử lý trả về Profile của Admin tại đây
                var user = await _profileRepo.GetUserByIdAsync(userId);
                if (user != null && user.Role == "Admin")
                {
                    return (true, "Lấy thông tin Admin thành công", new UserProfileResponse
                    {
                        FullName = string.IsNullOrEmpty(user.FullName) ? "Administrator" : user.FullName,
                        Email = user.Email,
                        Phone = user.Phone
                    });
                }
                return (false, "Không tìm thấy thông tin sinh viên", null);
            }

            var relative = student.Relatives.FirstOrDefault();

            var data = new UserProfileResponse
            {
                FullName = student.FullName,
                CitizenId = student.CitizenId,
                Gender = student.Gender,
                Phone = student.Phone,
                Email = student.Email,
                PermanentAddress = student.PermanentAddress,
                RelativeName = relative?.FullName ?? string.Empty,
                RelativePhone = relative?.Phone ?? string.Empty,
                Relationship = relative?.Relationship ?? string.Empty
            };

            return (true, "Lấy thông tin thành công", data);
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var user = await _profileRepo.GetUserByIdAsync(userId);
            if (user == null)
                return (false, "Không tìm thấy thông tin tài khoản");

            // Kiểm tra SĐT trùng với người khác trong hệ thống (KHÔNG tính bản thân mình)
            var phoneExists = await _profileRepo.PhoneExistsAsync(request.Phone, userId);
            if (phoneExists)
                return (false, "Số điện thoại đã tồn tại trong hệ thống");

            if (user.Role == "Admin")
            {
                user.Phone = request.Phone;
                await _profileRepo.UpdateUserAsync(user);
                return (true, "Cập nhật thông tin Admin thành công");
            }

            var student = await _profileRepo.GetStudentByUserIdAsync(userId);
            if (student == null)
                return (false, "Không tìm thấy thông tin sinh viên");

            // Validate student specific fields
            if (string.IsNullOrWhiteSpace(request.PermanentAddress))
                return (false, "Địa chỉ thường trú không được để trống");
            if (request.PermanentAddress.Length < 10)
                return (false, "Địa chỉ thường trú phải có ít nhất 10 ký tự");
            if (string.IsNullOrWhiteSpace(request.RelativeName))
                return (false, "Họ tên thân nhân không được để trống");
            if (string.IsNullOrWhiteSpace(request.RelativePhone))
                return (false, "Số điện thoại thân nhân không được để trống");
            if (string.IsNullOrWhiteSpace(request.Relationship))
                return (false, "Mối quan hệ không được để trống");

            // Cập nhật thông tin bản thân sinh viên
            student.Phone = request.Phone;
            student.PermanentAddress = request.PermanentAddress;

            // Cập nhật thông tin thân nhân
            var relative = student.Relatives.FirstOrDefault();
            
            if (relative == null)
            {
                if (student.Relatives == null)
                    student.Relatives = new List<Relative>();

                relative = new Relative
                {
                    FullName = request.RelativeName,
                    Phone = request.RelativePhone,
                    Relationship = request.Relationship,
                    StudentId = student.Id
                };

                student.Relatives.Add(relative);
            }
            else
            {
                relative.FullName = request.RelativeName;
                relative.Phone = request.RelativePhone;
                relative.Relationship = request.Relationship;
            }

            await _profileRepo.UpdateStudentAsync(student);

            return (true, "Cập nhật thông tin thành công");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _profileRepo.GetUserByIdAsync(userId);
            if (user == null)
                return (false, "Không tìm thấy tài khoản");

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                return (false, "Mật khẩu cũ không chính xác");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.MustChangePassword = false;
            await _profileRepo.UpdateUserAsync(user);

            return (true, "Đổi mật khẩu thành công");
        }
    }
}
