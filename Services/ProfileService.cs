using BackendAPI.Models.DTOs.Profile.Requests;
using BackendAPI.Models.DTOs.Profile.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepo;

        public ProfileService(IProfileRepository profileRepo)
        {
            _profileRepo = profileRepo;
        }

        public async Task<(bool Success, string Message, UserProfileResponse? Data)> GetProfileAsync(int userId)
        {
            var student = await _profileRepo.GetStudentByUserIdAsync(userId);

            if (student == null)
            {
                // N?u User là Admin, có th? x? lý tr? v? Profile c?a Admin t?i ?ây
                var user = await _profileRepo.GetUserByIdAsync(userId);
                if (user != null && user.Role == "Admin")
                {
                    return (true, "L?y thông tin Admin thành công", new UserProfileResponse
                    {
                        FullName = string.IsNullOrEmpty(user.FullName) ? "Administrator" : user.FullName,
                        Email = user.Email,
                        Phone = user.Phone
                    });
                }
                return (false, "Không tìm th?y thông tin sinh viên", null);
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

            return (true, "L?y thông tin thành công", data);
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            var student = await _profileRepo.GetStudentByUserIdAsync(userId);

            if (student == null)
                return (false, "Không tìm th?y thông tin sinh viên");

            // Ki?m tra S?T trùng v?i ng??i khác trong h? th?ng (KHÔNG tính b?n thân mình)
            var phoneExists = await _profileRepo.PhoneExistsAsync(request.Phone, userId);
            if (phoneExists)
                return (false, "S? ?i?n tho?i ?ã t?n t?i trong h? th?ng");

            // C?p nh?t thông tin b?n thân sinh viên
            student.Phone = request.Phone;
            student.PermanentAddress = request.PermanentAddress;

            // C?p nh?t thông tin thân nhân
            var relative = student.Relatives.FirstOrDefault();
            if (relative == null)
            {
                relative = new Relative { StudentId = student.Id };
                await _profileRepo.AddRelativeAsync(relative);
            }

            relative.FullName = request.RelativeName;
            relative.Phone = request.RelativePhone;
            relative.Relationship = request.Relationship;

            await _profileRepo.UpdateStudentAsync(student);

            return (true, "C?p nh?t thông tin thành công");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _profileRepo.GetUserByIdAsync(userId);
            if (user == null)
                return (false, "Không tìm th?y tài kho?n");

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.PasswordHash))
                return (false, "M?t kh?u c? không chính xác");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _profileRepo.UpdateUserAsync(user);

            return (true, "??i m?t kh?u thành công");
        }
    }
}
