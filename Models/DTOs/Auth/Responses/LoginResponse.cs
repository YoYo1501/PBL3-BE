namespace BackendAPI.Models.DTOs.Auth.Responses
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public bool MustChangePassword { get; set; }
    }
}
