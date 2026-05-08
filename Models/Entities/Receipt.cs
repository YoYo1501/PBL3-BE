namespace BackendAPI.Models.Entities;

public class Receipt
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string ReceiptCode { get; set; } = string.Empty;
    public decimal PaidAmount { get; set; }
    public DateTime PaidAt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionCode { get; set; } = string.Empty;
    public string Status { get; set; } = "Success";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Invoice Invoice { get; set; } = null!;
}
