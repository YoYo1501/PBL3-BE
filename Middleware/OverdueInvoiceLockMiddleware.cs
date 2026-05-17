using System.Security.Claims;
using System.Text.Json;
using BackendAPI.Repositories.Interfaces;

namespace BackendAPI.Middleware;

public class OverdueInvoiceLockMiddleware(RequestDelegate next)
{
    private static readonly string[] AllowedPrefixes =
    [
        "/api/invoices/my",
        "/api/payments",
        "/api/receipts/my",
        "/api/notifications",
        "/api/profile"
    ];

    public async Task InvokeAsync(HttpContext context, IInvoiceRepository invoiceRepository)
    {
        if (!ShouldCheck(context))
        {
            await next(context);
            return;
        }

        var studentIdValue = context.User.FindFirstValue("StudentId");
        if (!int.TryParse(studentIdValue, out var studentId))
        {
            await next(context);
            return;
        }

        if (!await invoiceRepository.HasOverdueUnpaidInvoiceAsync(studentId, DateTime.UtcNow))
        {
            await next(context);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status423Locked;
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = JsonSerializer.Serialize(new
        {
            message = "Tài khoản đang bị tạm khóa do có hóa đơn quá hạn. Vui lòng thanh toán hóa đơn để tiếp tục sử dụng các chức năng.",
            code = "OVERDUE_INVOICE_LOCKED"
        });

        await context.Response.WriteAsync(payload);
    }

    private static bool ShouldCheck(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
            return false;

        if (!context.User.IsInRole("Student"))
            return false;

        if (!context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            return false;

        var path = context.Request.Path.Value ?? string.Empty;
        return !AllowedPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
}
