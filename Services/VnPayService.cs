using BackendAPI.Helpers;
using BackendAPI.Models.DTOs.Payment.Requests;
using BackendAPI.Models.DTOs.Payment.Responses;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Services;

public class VnPayService : IPaymentService
{
    private readonly IConfiguration _configuration;

    public VnPayService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // ================= CREATE URL =================
    public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);

        var pay = new VnPayLibrary();

        var vnpReturnUrl = _configuration["VnPay:ReturnUrl"]!.Trim();
        var vnpHashSecret = _configuration["VnPay:HashSecret"]!.Trim();
        var vnpTmnCode = _configuration["VnPay:TmnCode"]!.Trim();
        var vnpBaseUrl = _configuration["VnPay:BaseUrl"]!.Trim();

        pay.AddRequestData("vnp_Version", "2.1.0");
        pay.AddRequestData("vnp_Command", "pay");
        pay.AddRequestData("vnp_TmnCode", vnpTmnCode);

        // FIX chuẩn tiền
        pay.AddRequestData("vnp_Amount", ((long)Math.Round(model.Amount * 100)).ToString());

        pay.AddRequestData("vnp_CreateDate", now.ToString("yyyyMMddHHmmss"));
        pay.AddRequestData("vnp_CurrCode", "VND");

        // FIX IP
        pay.AddRequestData("vnp_IpAddr", VnPayLibrary.GetIpAddress(context));

        pay.AddRequestData("vnp_Locale", "vn");
        pay.AddRequestData("vnp_OrderInfo", $"{model.Name}_{model.OrderDescription}_{model.Amount}");
        pay.AddRequestData("vnp_OrderType", model.OrderType);
        pay.AddRequestData("vnp_ReturnUrl", vnpReturnUrl);

        pay.AddRequestData("vnp_TxnRef", $"{model.InvoiceId}_{DateTime.Now.Ticks}");
        pay.AddRequestData("vnp_ExpireDate", now.AddMinutes(15).ToString("yyyyMMddHHmmss"));

        return pay.CreateRequestUrl(vnpBaseUrl, vnpHashSecret);
    }

    // ================= HANDLE RESPONSE =================
    public PaymentResponseModel PaymentExecute(IQueryCollection collections)
    {
        var pay = new VnPayLibrary();

        foreach (var (key, value) in collections)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                pay.AddResponseData(key, value.ToString());
            }
        }

        var vnpHash = collections["vnp_SecureHash"];

        var isValid = pay.ValidateSignature(vnpHash!, _configuration["VnPay:HashSecret"]!);

        if (!isValid)
        {
            return new PaymentResponseModel
            {
                Success = false
            };
        }

        var orderIdStr = pay.GetResponseData("vnp_TxnRef");
        var invoiceId = orderIdStr.Split('_')[0];

        return new PaymentResponseModel
        {
            Success = pay.GetResponseData("vnp_ResponseCode") == "00",
            PaymentMethod = "VnPay",
            OrderDescription = pay.GetResponseData("vnp_OrderInfo"),
            OrderId = invoiceId,
            TransactionId = pay.GetResponseData("vnp_TransactionNo"),
            VnPayResponseCode = pay.GetResponseData("vnp_ResponseCode")
        };
    }
}