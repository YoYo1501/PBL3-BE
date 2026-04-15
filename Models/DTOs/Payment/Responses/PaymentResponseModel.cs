namespace BackendAPI.Models.DTOs.Payment.Responses;

public class PaymentResponseModel
{
    public string OrderDescription { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string OrderId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public string VnPayResponseCode { get; set; } = string.Empty;
    public bool Success { get; set; }
}