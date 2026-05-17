namespace BackendAPI.Services.Interfaces;

public interface IEmailService
{
    Task SendAccountInfoAsync(string email, string fullName, string citizenId);
    Task SendRegistrationApprovedAsync(string email, string fullName, string roomCode, DateTime startDate, DateTime endDate);
    Task SendRegistrationRejectedAsync(string email, string fullName, string reason);
}
