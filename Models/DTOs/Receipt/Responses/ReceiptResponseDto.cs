namespace BackendAPI.Models.DTOs.Receipt.Responses;

public class ReceiptResponseDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string ReceiptCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public decimal RoomFee { get; set; }
    public decimal ElectricFee { get; set; }
    public decimal WaterFee { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime PaidAt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionCode { get; set; } = string.Empty;
}
