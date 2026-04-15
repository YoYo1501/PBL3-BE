using BackendAPI.Models.DTOs.Payment.Requests;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentsController(IPaymentService _paymentService, IInvoiceRepository _invoiceRepo) : ControllerBase
{
    [HttpPost("create-payment-url/{invoiceId}")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> CreatePaymentUrl(int invoiceId)
    {
        var invoice = await _invoiceRepo.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
            return NotFound(new { message = "Hóa ??n không t?n t?i" });

        if (invoice.Status == "Paid")
            return BadRequest(new { message = "Hóa ??n ?ã ???c thanh toán" });

        var model = new PaymentInformationModel
        {
            InvoiceId = invoiceId,
            Amount = (double)invoice.TotalAmount,
            OrderType = "billpayment",
            OrderDescription = $"Thanh_toan_hoa_don_{invoiceId}",
            Name = "SinhVien"
        };

        var url = _paymentService.CreatePaymentUrl(model, HttpContext);
        return Ok(new { url });
    }

    [HttpGet("vnpay-return")]
    public async Task<IActionResult> PaymentCallback()
    {
        var response = _paymentService.PaymentExecute(Request.Query);

        if (response.Success)
        {
            if (int.TryParse(response.OrderId, out int invoiceId))
            {
                var invoice = await _invoiceRepo.GetInvoiceByIdAsync(invoiceId);
        if (invoice != null && invoice.Status != "Paid")
                {
                    invoice.Status = "Paid";
                    await _invoiceRepo.UpdateInvoiceAsync(invoice);
                    await _invoiceRepo.SaveChangesAsync();
                }
            }
            return Ok(new { message = "Thanh toán thành công", data = response });
        }

        return BadRequest(new { message = "Thanh toán th?t b?i ho?c b? h?y", data = response });
    }
}
