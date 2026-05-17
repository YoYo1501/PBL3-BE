namespace BackendAPI.Models.DTOs.Invoice.Responses;

public class InvoiceDraftDto
{
    public int Id { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
    public string RoomCode { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public decimal RoomFee { get; set; }
    public decimal ElectricFee { get; set; }
    public decimal WaterFee { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionCode { get; set; }
}
