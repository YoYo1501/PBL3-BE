using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Receipt.Requests;
using BackendAPI.Models.DTOs.Receipt.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Helpers;
using BackendAPI.Services.Interfaces;
using BackendAPI.Repositories.Interfaces;
using OfficeOpenXml;

namespace BackendAPI.Services;

public class ReceiptService(IReceiptRepository receiptRepository) : IReceiptService
{
    public async Task<List<ReceiptResponseDto>> GetMyReceiptsAsync(int studentId, string? period)
    {
        var receipts = await receiptRepository.GetReceiptsAsync(NormalizePeriod(period), studentId);
        return receipts.Select(ToDto).ToList();
    }

    public async Task<PagedResultDto<ReceiptResponseDto>> GetPagedMyReceiptsAsync(int studentId, ReceiptListQueryDto query)
    {
        query.Period = NormalizePeriod(query.Period);
        var page = query.GetPage();
        var pageSize = query.GetPageSize();
        var (items, totalCount) = await receiptRepository.GetPagedReceiptsAsync(query.Period, studentId, page, pageSize);

        return BuildPagedResult(items, page, pageSize, totalCount);
    }

    public async Task<ReceiptResponseDto?> GetMyReceiptByInvoiceIdAsync(int studentId, int invoiceId)
    {
        var receipt = await receiptRepository.GetReceiptByInvoiceIdAsync(invoiceId, studentId);
        return receipt == null ? null : ToDto(receipt);
    }

    public async Task<ReceiptExportDto?> ExportMyReceiptAsync(int studentId, int invoiceId)
    {
        var receipt = await receiptRepository.GetReceiptByInvoiceIdAsync(invoiceId, studentId);
        return receipt == null ? null : BuildReceiptFile(receipt);
    }

    public async Task<List<ReceiptResponseDto>> GetAllReceiptsAsync(string? period, int? studentId)
    {
        var receipts = await receiptRepository.GetReceiptsAsync(NormalizePeriod(period), studentId);
        return receipts.Select(ToDto).ToList();
    }

    public async Task<PagedResultDto<ReceiptResponseDto>> GetPagedReceiptsAsync(ReceiptListQueryDto query)
    {
        query.Period = NormalizePeriod(query.Period);
        var page = query.GetPage();
        var pageSize = query.GetPageSize();
        var (items, totalCount) = await receiptRepository.GetPagedReceiptsAsync(query.Period, query.StudentId, page, pageSize);

        return BuildPagedResult(items, page, pageSize, totalCount);
    }

    public async Task<ReceiptResponseDto?> GetReceiptByInvoiceIdAsync(int invoiceId)
    {
        var receipt = await receiptRepository.GetReceiptByInvoiceIdAsync(invoiceId);
        return receipt == null ? null : ToDto(receipt);
    }

    public async Task<ReceiptExportDto?> ExportReceiptAsync(int invoiceId)
    {
        var receipt = await receiptRepository.GetReceiptByInvoiceIdAsync(invoiceId);
        return receipt == null ? null : BuildReceiptFile(receipt);
    }

    private static PagedResultDto<ReceiptResponseDto> BuildPagedResult(List<Receipt> items, int page, int pageSize, int totalCount)
        => new()
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize))
        };

    private static ReceiptResponseDto ToDto(Receipt receipt)
    {
        var invoice = receipt.Invoice;

        return new ReceiptResponseDto
        {
            Id = receipt.Id,
            InvoiceId = invoice.Id,
            ReceiptCode = receipt.ReceiptCode,
            StudentName = invoice.Student?.FullName ?? string.Empty,
            RoomCode = invoice.Room?.RoomCode ?? string.Empty,
            Period = invoice.Period,
            RoomFee = invoice.RoomFee,
            ElectricFee = invoice.ElectricFee,
            WaterFee = invoice.WaterFee,
            TotalAmount = invoice.TotalAmount,
            PaidAmount = receipt.PaidAmount,
            Status = receipt.Status,
            IssuedAt = VietnamTime.FromUtc(invoice.IssuedAt),
            PaidAt = VietnamTime.FromUtc(receipt.PaidAt),
            PaymentMethod = string.IsNullOrWhiteSpace(receipt.PaymentMethod) ? "Unknown" : receipt.PaymentMethod,
            TransactionCode = receipt.TransactionCode ?? string.Empty
        };
    }

    private static ReceiptExportDto BuildReceiptFile(Receipt receipt)
    {
        var receiptDto = ToDto(receipt);

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("BienLai");

        worksheet.Cells[1, 1].Value = "BIEN LAI THANH TOAN";
        worksheet.Cells[1, 1, 1, 4].Merge = true;
        worksheet.Cells[1, 1, 1, 4].Style.Font.Bold = true;
        worksheet.Cells[1, 1, 1, 4].Style.Font.Size = 16;

        worksheet.Cells[3, 1].Value = "Ma bien lai";
        worksheet.Cells[3, 2].Value = receiptDto.ReceiptCode;
        worksheet.Cells[4, 1].Value = "Ma hoa don";
        worksheet.Cells[4, 2].Value = receiptDto.InvoiceId;
        worksheet.Cells[5, 1].Value = "Sinh vien";
        worksheet.Cells[5, 2].Value = receiptDto.StudentName;
        worksheet.Cells[6, 1].Value = "Phong";
        worksheet.Cells[6, 2].Value = receiptDto.RoomCode;
        worksheet.Cells[7, 1].Value = "Ky thanh toan";
        worksheet.Cells[7, 2].Value = receiptDto.Period;
        worksheet.Cells[8, 1].Value = "Ngay phat hanh";
        worksheet.Cells[8, 2].Value = receiptDto.IssuedAt.ToString("dd/MM/yyyy HH:mm:ss");
        worksheet.Cells[9, 1].Value = "Ngay thanh toan";
        worksheet.Cells[9, 2].Value = receiptDto.PaidAt.ToString("dd/MM/yyyy HH:mm:ss");
        worksheet.Cells[10, 1].Value = "Phuong thuc";
        worksheet.Cells[10, 2].Value = receiptDto.PaymentMethod;
        worksheet.Cells[11, 1].Value = "Ma giao dich";
        worksheet.Cells[11, 2].Value = receiptDto.TransactionCode;

        worksheet.Cells[13, 1].Value = "Khoan thu";
        worksheet.Cells[13, 2].Value = "So tien";
        worksheet.Cells[13, 1, 13, 2].Style.Font.Bold = true;
        worksheet.Cells[14, 1].Value = "Tien phong";
        worksheet.Cells[14, 2].Value = receiptDto.RoomFee;
        worksheet.Cells[15, 1].Value = "Tien dien";
        worksheet.Cells[15, 2].Value = receiptDto.ElectricFee;
        worksheet.Cells[16, 1].Value = "Tien nuoc";
        worksheet.Cells[16, 2].Value = receiptDto.WaterFee;
        worksheet.Cells[17, 1].Value = "Tong cong";
        worksheet.Cells[17, 2].Value = receiptDto.PaidAmount;
        worksheet.Cells[17, 1, 17, 2].Style.Font.Bold = true;

        worksheet.Cells[14, 2, 17, 2].Style.Numberformat.Format = "#,##0";
        worksheet.Cells.AutoFitColumns();

        return new ReceiptExportDto
        {
            Content = package.GetAsByteArray(),
            FileName = $"BienLai_{receiptDto.ReceiptCode}.xlsx"
        };
    }

    private static string? NormalizePeriod(string? period)
        => string.IsNullOrWhiteSpace(period) ? null : period.Trim();
}
