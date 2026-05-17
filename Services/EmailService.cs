using System.Net;
using System.Net.Mail;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    private (string Host, int Port, string? User, string? Password) GetSmtpSettings()
    {
        var smtpHost = config["SmtpSettings:Host"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(config["SmtpSettings:Port"] ?? "587");
        var smtpUser = config["SmtpSettings:Username"];
        var smtpPass = config["SmtpSettings:Password"];
        return (smtpHost, smtpPort, smtpUser, smtpPass);
    }

    private async Task SendHtmlAsync(string email, string subject, string body)
    {
        var (smtpHost, smtpPort, smtpUser, smtpPass) = GetSmtpSettings();

        if (string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
        {
            Console.WriteLine("Cảnh báo: Chưa cấu hình SMTP Username và Password.");
            return;
        }

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpUser, "Ký Túc Xá"),
            Subject = subject,
            Body = body,
            IsBodyHtml = true,
        };
        mailMessage.To.Add(email);

        using var smtpClient = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(smtpUser, smtpPass),
            EnableSsl = true,
        };

        try
        {
            await smtpClient.SendMailAsync(mailMessage);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Lỗi khi gửi email: {ex.Message}");
        }
    }

    public Task SendAccountInfoAsync(string email, string fullName, string citizenId)
        => SendHtmlAsync(
            email,
            "Đăng ký cư trú tại KTX thành công",
            $"""
            <h2>Chào bạn {WebUtility.HtmlEncode(fullName)},</h2>
            <p>Đơn đăng ký của bạn đã được ban quản lý ký túc xá phê duyệt.</p>
            <p>Dưới đây là thông tin tài khoản để bạn đăng nhập vào hệ thống Ký Túc Xá:</p>
            <ul>
                <li><strong>Tên đăng nhập:</strong> {WebUtility.HtmlEncode(citizenId)}</li>
                <li><strong>Mật khẩu mặc định:</strong> {WebUtility.HtmlEncode(citizenId)}</li>
            </ul>
            <p>Bạn vui lòng đổi mật khẩu ngay sau lần đăng nhập đầu tiên để đảm bảo an toàn.</p>
            <p>Trân trọng,<br>Ban Quản Lý KTX</p>
            """);

    public Task SendRegistrationApprovedAsync(string email, string fullName, string roomCode, DateTime startDate, DateTime endDate)
        => SendHtmlAsync(
            email,
            "Đơn đăng ký ở KTX đã được phê duyệt",
            $"""
            <h2>Chào bạn {WebUtility.HtmlEncode(fullName)},</h2>
            <p>Đơn đăng ký ở ký túc xá của bạn đã được phê duyệt.</p>
            <ul>
                <li><strong>Phòng:</strong> {WebUtility.HtmlEncode(roomCode)}</li>
                <li><strong>Ngày bắt đầu:</strong> {startDate:dd/MM/yyyy}</li>
                <li><strong>Ngày kết thúc:</strong> {endDate:dd/MM/yyyy}</li>
            </ul>
            <p>Bạn vui lòng đăng nhập hệ thống để xem hợp đồng và các thông tin lưu trú.</p>
            <p>Trân trọng,<br>Ban Quản Lý KTX</p>
            """);

    public Task SendRegistrationRejectedAsync(string email, string fullName, string reason)
        => SendHtmlAsync(
            email,
            "Đơn đăng ký ở KTX chưa được phê duyệt",
            $"""
            <h2>Chào bạn {WebUtility.HtmlEncode(fullName)},</h2>
            <p>Rất tiếc, đơn đăng ký ở ký túc xá của bạn chưa được phê duyệt.</p>
            <p><strong>Lý do:</strong> {WebUtility.HtmlEncode(reason)}</p>
            <p>Bạn có thể kiểm tra lại thông tin và gửi đơn đăng ký mới nếu cần.</p>
            <p>Trân trọng,<br>Ban Quản Lý KTX</p>
            """);
}
