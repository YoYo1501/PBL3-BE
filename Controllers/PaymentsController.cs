using System.Text;
using BackendAPI.Models.DTOs.Payment.Requests;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService paymentService, IInvoiceRepository invoiceRepo) : ControllerBase
{
    [HttpPost("create-payment-url/{invoiceId}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CreatePaymentUrl(int invoiceId)
    {
        var invoice = await invoiceRepo.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
            return NotFound(new { message = "Hóa đơn không tồn tại." });

        if (invoice.Status == "Paid")
            return BadRequest(new { message = "Hóa đơn đã được thanh toán." });

        var model = new PaymentInformationModel
        {
            InvoiceId = invoiceId,
            Amount = (double)invoice.TotalAmount,
            OrderType = "billpayment",
            OrderDescription = $"Thanh_toan_hoa_don_{invoiceId}",
            Name = "SinhVien",
            ReturnPage = Request.Query["returnPage"].ToString()
        };

        var url = paymentService.CreatePaymentUrl(model, HttpContext);
        return Ok(new { url });
    }

    [HttpGet("vnpay-return")]
    public async Task<IActionResult> PaymentCallback()
    {
        var response = paymentService.PaymentExecute(Request.Query);
        var returnPage = Request.Query["returnPage"].ToString();

        if (response.Success)
        {
            if (int.TryParse(response.OrderId, out var invoiceId))
            {
                var invoice = await invoiceRepo.GetInvoiceByIdAsync(invoiceId);
                if (invoice != null && invoice.Status != "Paid")
                {
                    invoice.Status = "Paid";
                    await invoiceRepo.UpdateInvoiceAsync(invoice);
                    await invoiceRepo.SaveChangesAsync();
                }
            }

            TryBuildStudentReturnUrl(returnPage, response, true, out var successUrl);

            return Content(
                BuildPaymentResultHtml(
                    success: true,
                    title: "Thanh toán thành công",
                    message: "Hóa đơn của bạn đã được ghi nhận thanh toán thành công qua VNPAY.",
                    invoiceId: response.OrderId,
                    transactionId: response.TransactionId,
                    responseCode: response.VnPayResponseCode,
                    redirectUrl: successUrl),
                "text/html; charset=utf-8",
                Encoding.UTF8);
        }

        TryBuildStudentReturnUrl(returnPage, response, false, out var failedUrl);

        return Content(
            BuildPaymentResultHtml(
                success: false,
                title: "Thanh toán chưa hoàn tất",
                message: "Giao dịch thất bại hoặc đã bị hủy. Bạn có thể quay lại mục hóa đơn để thử lại.",
                invoiceId: response.OrderId,
                transactionId: response.TransactionId,
                responseCode: response.VnPayResponseCode,
                redirectUrl: failedUrl),
            "text/html; charset=utf-8",
            Encoding.UTF8);
    }

    private static bool TryBuildStudentReturnUrl(
        string? returnPage,
        BackendAPI.Models.DTOs.Payment.Responses.PaymentResponseModel response,
        bool success,
        out string redirectUrl)
    {
        redirectUrl = string.Empty;
        if (string.IsNullOrWhiteSpace(returnPage))
            return false;

        if (!Uri.TryCreate(returnPage, UriKind.Absolute, out var uri))
            return false;

        if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            return false;

        var queryParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(uri.Query))
        {
            queryParts.Add(uri.Query.TrimStart('?'));
        }

        queryParts.Add($"paymentStatus={(success ? "success" : "failed")}");
        queryParts.Add($"paymentInvoiceId={Uri.EscapeDataString(response.OrderId ?? string.Empty)}");
        queryParts.Add($"paymentTxnId={Uri.EscapeDataString(response.TransactionId ?? string.Empty)}");
        queryParts.Add($"paymentCode={Uri.EscapeDataString(response.VnPayResponseCode ?? string.Empty)}");

        redirectUrl = $"{uri.GetLeftPart(UriPartial.Path)}?{string.Join("&", queryParts.Where(part => !string.IsNullOrWhiteSpace(part)))}{uri.Fragment}";
        return true;
    }

    private static string BuildPaymentResultHtml(
        bool success,
        string title,
        string message,
        string? invoiceId,
        string? transactionId,
        string? responseCode,
        string? redirectUrl)
    {
        var accent = success ? "#1f8b4c" : "#c2410c";
        var soft = success ? "#e8f7ee" : "#fff1e8";
        var icon = success ? "✓" : "!";
        var details = new StringBuilder();
        var hasRedirect = !string.IsNullOrWhiteSpace(redirectUrl);

        if (!string.IsNullOrWhiteSpace(invoiceId))
            details.Append($"<div class='detail-row'><span>Hóa đơn</span><strong>#{invoiceId}</strong></div>");
        if (!string.IsNullOrWhiteSpace(transactionId))
            details.Append($"<div class='detail-row'><span>Mã giao dịch</span><strong>{transactionId}</strong></div>");
        if (!string.IsNullOrWhiteSpace(responseCode))
            details.Append($"<div class='detail-row'><span>Mã phản hồi</span><strong>{responseCode}</strong></div>");

        return $$"""
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>{{title}}</title>
    <style>
        * { box-sizing: border-box; }
        body {
            margin: 0;
            min-height: 100vh;
            display: grid;
            place-items: center;
            padding: 24px;
            font-family: Arial, Helvetica, sans-serif;
            background:
                radial-gradient(circle at top, rgba(74, 146, 192, 0.18), transparent 38%),
                linear-gradient(180deg, #f8fbfd, #edf4f8);
            color: #24435f;
        }
        .result-card {
            width: min(100%, 560px);
            background: #fff;
            border: 1px solid #d9e3ea;
            border-radius: 24px;
            padding: 28px;
            box-shadow: 0 24px 60px rgba(31, 67, 95, 0.14);
        }
        .icon {
            width: 72px;
            height: 72px;
            display: grid;
            place-items: center;
            margin: 0 auto 18px;
            border-radius: 20px;
            background: {{soft}};
            color: {{accent}};
            font-size: 36px;
            font-weight: 700;
        }
        h1 {
            margin: 0 0 10px;
            text-align: center;
            color: {{accent}};
            font-size: 30px;
        }
        p {
            margin: 0;
            text-align: center;
            line-height: 1.6;
            color: #526577;
        }
        .redirect-note {
            margin-top: 14px;
            color: #607384;
            font-size: 14px;
        }
        .details {
            margin-top: 22px;
            border-top: 1px solid #ebf0f4;
            padding-top: 16px;
            display: grid;
            gap: 10px;
        }
        .detail-row {
            display: flex;
            justify-content: space-between;
            gap: 12px;
            padding: 10px 12px;
            border-radius: 14px;
            background: #f8fbfd;
        }
        .detail-row span { color: #6a7b89; }
        .detail-row strong { color: #1f3447; }
        .actions {
            display: flex;
            justify-content: center;
            gap: 12px;
            flex-wrap: wrap;
            margin-top: 22px;
        }
        .btn {
            appearance: none;
            border: 0;
            border-radius: 14px;
            padding: 12px 18px;
            font-size: 15px;
            cursor: pointer;
        }
        .btn-primary {
            background: {{accent}};
            color: #fff;
        }
        .btn-secondary {
            background: #edf3f7;
            color: #28445e;
        }
    </style>
</head>
<body>
    <div class="result-card">
        <div class="icon">{{icon}}</div>
        <h1>{{title}}</h1>
        <p>{{message}}</p>
        <p class="redirect-note" {{(hasRedirect ? "" : "style='display:none'")}}>
            Hệ thống sẽ tự quay về trang sinh viên sau <strong id="countdown">3</strong> giây.
        </p>
        <div class="details">
            {{details}}
        </div>
        <div class="actions">
            <button class="btn btn-primary" type="button" onclick="{{(hasRedirect ? $"window.location.href='{redirectUrl}'" : "window.close()")}}">{{(hasRedirect ? "Về trang sinh viên ngay" : "Đóng trang này")}}</button>
            <button class="btn btn-secondary" type="button" onclick="window.history.back()">Quay lại</button>
        </div>
    </div>
    <script>
        (function () {
            var redirectUrl = {{(hasRedirect ? $"'{redirectUrl}'" : "''")}};
            if (!redirectUrl) return;
            var countdownEl = document.getElementById('countdown');
            var remaining = 3;
            var timer = window.setInterval(function () {
                remaining -= 1;
                if (countdownEl && remaining >= 0) countdownEl.textContent = String(remaining);
                if (remaining <= 0) {
                    window.clearInterval(timer);
                    window.location.href = redirectUrl;
                }
            }, 1000);
        })();
    </script>
</body>
</html>
""";
    }
}
