using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Receipt.Requests;
using BackendAPI.Models.DTOs.Receipt.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using OfficeOpenXml;

namespace BackendAPI.Services;

public class ReceiptService(IInvoiceRepository invoiceRepository) : IReceiptService
{
    public async Task<List<ReceiptResponseDto>> GetMyReceiptsAsync(int studentId, string? period)
    {
        var invoices = await invoiceRepository.GetPaidInvoicesAsync(NormalizePeriod(period), studentId);
        return invoices.Select(ToDto).ToList();
    }

    public async Task<PagedResultDto<ReceiptResponseDto>> GetPagedMyReceiptsAsync(int studentId, ReceiptListQueryDto query)
    {
        query.Period = NormalizePeriod(query.Period);
        var page = query.GetPage();
        var pageSize = query.GetPageSize();
        var (items, totalCount) = await invoiceRepository.GetPagedPaidInvoicesAsync(query.Period, studentId, page, pageSize);

        return BuildPagedResult(items, page, pageSize, totalCount);
    }

    public async Task<ReceiptResponseDto?> GetMyReceiptByInvoiceIdAsync(int studentId, int invoiceId)
    {
        var invoice = await invoiceRepository.GetPaidInvoiceByIdAsync(invoiceId, studentId);
        return invoice == null ? null : ToDto(invoice);
    }

    public async Task<ReceiptExportDto?> ExportMyReceiptAsync(int studentId, int invoiceId)
    {
        var invoice = await invoiceRepository.GetPaidInvoiceByIdAsync(invoiceId, studentId);
        return invoice == null ? null : BuildReceiptFile(invoice);
    }

    public async Task<List<ReceiptResponseDto>> GetAllReceiptsAsync(string? period, int? studentId)
    {
        var invoices = await invoiceRepository.GetPaidInvoicesAsync(NormalizePeriod(period), studentId);
        return invoices.Select(ToDto).ToList();
    }

    public async Task<PagedResultDto<ReceiptResponseDto>> GetPagedReceiptsAsync(ReceiptListQueryDto query)
    {
        query.Period = NormalizePeriod(query.Period);
        var page = query.GetPage();
        var pageSize = query.GetPageSize();
        var (items, totalCount) = await invoiceRepository.GetPagedPaidInvoicesAsync(query.Period, query.StudentId, page, pageSize);

        return BuildPagedResult(items, page, pageSize, totalCount);
    }

    public async Task<ReceiptResponseDto?> GetReceiptByInvoiceIdAsync(int invoiceId)
    {
        var invoice = await invoiceRepository.GetPaidInvoiceByIdAsync(invoiceId);
        return invoice == null ? null : ToDto(invoice);
    }

    public async Task<ReceiptExportDto?> ExportReceiptAsync(int invoiceId)
    {
        var invoice = await invoiceRepository.GetPaidInvoiceByIdAsync(invoiceId);
        return invoice == null ? null : BuildReceiptFile(invoice);
    }

    private static PagedResultDto<ReceiptResponseDto> BuildPagedResult(List<Invoice> items, int page, int pageSize, int totalCount)
        => new()
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize))
        };

    private static ReceiptResponseDto ToDto(Invoice invoice)
    {
        var paidAt = invoice.PaidAt ?? invoice.IssuedAt;

        return new ReceiptResponseDto
        {
            Id = invoice.Id,
            InvoiceId = invoice.Id,
            ReceiptCode = BuildReceiptCode(invoice),
            StudentName = invoice.Student?.FullName ?? string.Empty,
            RoomCode = invoice.Room?.RoomCode ?? string.Empty,
            Period = invoice.Period,
            RoomFee = invoice.RoomFee,
            ElectricFee = invoice.ElectricFee,
            WaterFee = invoice.WaterFee,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = invoice.TotalAmount,
            Status = invoice.Status,
            IssuedAt = invoice.IssuedAt,
            PaidAt = paidAt,
            PaymentMethod = string.IsNullOrWhiteSpace(invoice.PaymentMethod) ? "Unknown" : invoice.PaymentMethod,
            TransactionCode = invoice.TransactionCode ?? string.Empty
        };
    }

    private static ReceiptExportDto BuildReceiptFile(Invoice invoice)
    {
        var receipt = ToDto(invoice);

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("BienLai");

        worksheet.Cells[1, 1].Value = "BIEN LAI THANH TOAN";
        worksheet.Cells[1, 1, 1, 4].Merge = true;
        worksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;
        worksheet.Cells[1, 1, 1, 4].Style.Font.Size = 16;

        worksheet.Cells[3, 1].Value = "Ma bien lai";
        worksheet.Cells[3, 2].Value = receipt.ReceiptCode;
        worksheet.Cells[4, 1].Value = "Ma hoa don";
        worksheet.Cells[4, 2].Value = receipt.InvoiceId;
        worksheet.Cells[5, 1].Value = "Sinh vien";
        worksheet.Cells[5, 2].Value = receipt.StudentName;
        worksheet.Cells[6, 1].Value = "Phong";
        worksheet.Cells[6, 2].Value = receipt.RoomCode;
        worksheet.Cells[7, 1].Value = "Ky thanh toan";
        worksheet.Cells[7, 2].Value = receipt.Period;
        worksheet.Cells[8, 1].Value = "Ngay phat hanh";
        worksheet.Cells[8, 2].Value = receipt.IssuedAt.ToString("dd/MM/yyyy HH:mm:ss");
        worksheet.Cells[9, 1].Value = "Ngay thanh toan";
        worksheet.Cells[9, 2].Value = receipt.PaidAt.ToString("dd/MM/yyyy HH:mm:ss");
        worksheet.Cells[10, 1].Value = "Phuong thuc";
        worksheet.Cells[10, 2].Value = receipt.PaymentMethod;
        worksheet.Cells[11, 1].Value = "Ma giao dich";
        worksheet.Cells[11, 2].Value = receipt.TransactionCode;

        worksheet.Cells[13, 1].Value = "Khoan thu";
        worksheet.Cells[13, 2].Value = "So tien";
        worksheet.Cells[13, 1, 13, 2].Style.Font.Bold = true;
        worksheet.Cells[14, 1].Value = "Tien phong";
        worksheet.Cells[14, 2].Value = receipt.RoomFee;
        worksheet.Cells[15, 1].Value = "Tien dien";
        worksheet.Cells[15, 2].Value = receipt.ElectricFee;
        worksheet.Cells[16, 1].Value = "Tien nuoc";
        worksheet.Cells[16, 2].Value = receipt.WaterFee;
        worksheet.Cells[17, 1].Value = "Tong cong";
        worksheet.Cells[17, 2].Value = receipt.PaidAmount;
        worksheet.Cells[17, 1, 17, 2].Style.Font.Bold = true;

        worksheet.Cells[14, 2, 17, 2].Style.Numberformat.Format = "#,##0";
        worksheet.Cells.AutoFitColumns();

        return new ReceiptExportDto
        {
            Content = package.GetAsByteArray(),
            FileName = $"BienLai_{receipt.ReceiptCode}.xlsx"
        };
    }

    private static string BuildReceiptCode(Invoice invoice)
    {
        var period = invoice.Period
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("/", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);

        return $"BL-{period}-{invoice.Id:D6}";
    }

    private static string? NormalizePeriod(string? period)
        => string.IsNullOrWhiteSpace(period) ? null : period.Trim();
}
