namespace BackendAPI.Models.DTOs.Profile.Responses
{
    public class UserProfileResponse
    {
        public string FullName { get; set; } = string.Empty;
        public string CitizenId { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PermanentAddress { get; set; } = string.Empty;
        
        // Thông tin thân nhân
        public string RelativeName { get; set; } = string.Empty;
        public string RelativePhone { get; set; } = string.Empty;
        public string Relationship { get; set; } = string.Empty;
    }
}
