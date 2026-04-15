namespace BackendAPI.Services.Interfaces;

public interface IEmailService
{
    Task SendAccountInfoAsync(string email, string fullName, string citizenId);
}