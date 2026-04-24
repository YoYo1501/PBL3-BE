using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class EmailService(IConfiguration _config) : IEmailService
{
    public async Task SendAccountInfoAsync(string email, string fullName, string citizenId)
    {
        var smtpHost = _config["SmtpSettings:Host"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(_config["SmtpSettings:Port"] ?? "587");
        var smtpUser = _config["SmtpSettings:Username"];
        var smtpPass = _config["SmtpSettings:Password"];
        
        if(string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
        {
            // Trong tr??ng h?p này, ch?a c?u hình SMTP thì b? qua, ho?c in ra log.
            Console.WriteLine("C?nh báo: Ch?a c?u hình SMTP Username và Password");
            return;
        }

        var mailMessage = new MailMessage
        {
            From = new MailAddress(smtpUser, "Ký Túc Xá"),
            Subject = "??ng ký c? trú t?i KTX thành công",
            Body = $@"
                <h2>Chào b?n {fullName},</h2>
                <p>??n ??ng ký c?a b?n ?ã ???c ban qu?n lý ký túc xá phê duy?t.</p>
                <p>D??i ?ây là thông tin tài kho?n ?? b?n ??ng nh?p vào h? th?ng ?ng d?ng Ký Túc Xá:</p>
                <ul>
                    <li><strong>Tên ??ng nh?p (Email / S?T / CCCD):</strong> {citizenId} ho?c {email}</li>
                    <li><strong>M?t kh?u m?c ??nh:</strong> {citizenId}</li>
                </ul>
                <p>B?n vui lòng thay ??i m?t kh?u ngay sau l?n ??ng nh?p ??u tiên ?? ??m b?o an toàn nhé.</p>
                <p>Trân tr?ng,<br>Ban Qu?n Lý KTX</p>
            ",
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
            Console.WriteLine($"L?i khi g?i email: {ex.Message}");
            // Có th? throw, ho?c ch? ghi log
        }
    }
}