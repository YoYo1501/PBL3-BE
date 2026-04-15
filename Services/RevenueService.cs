using BackendAPI.Models.DTOs.Revenue.Requests;
using BackendAPI.Models.DTOs.Revenue.Responses;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace BackendAPI.Services;

public class RevenueService(IRevenueRepository repo) : IRevenueService
{
    public async Task<(bool Success, string Message, RevenueResponseDto? Data)> GetRevenueAsync(RevenueFilterDto filter)
    {
        if (filter.StartDate > filter.EndDate)
            return (false, "Ngày bắt đầu phải nhỏ hơn ngày kết thúc.", null);

        if (filter.EndDate > DateTime.UtcNow)
            return (false, "Ngày kết thúc không được vượt quá ngày hiện tại.", null);

        var invoices = await repo.GetInvoicesAsync(
            filter.StartDate, filter.EndDate,
            filter.RoomCode, filter.Period);

        if (!invoices.Any())
            return (false, "Không có dữ liệu doanh thu trong khoảng thời gian này.", null);

        var result = new RevenueResponseDto
        {
            TotalRoomFee = invoices.Sum(i => i.RoomFee),
            TotalElectricFee = invoices.Sum(i => i.ElectricFee),
            TotalWaterFee = invoices.Sum(i => i.WaterFee),
            GrandTotal = invoices.Sum(i => i.TotalAmount),
            TotalInvoices = invoices.Count,
            PaidInvoices = invoices.Count(i => i.Status == "Paid"),
            UnpaidInvoices = invoices.Count(i => i.Status == "Unpaid"),
            Details = invoices.Select(i => new RevenueDetailDto
            {
                Period = i.Period,
                RoomCode = i.Room.RoomCode,
                StudentName = i.Student.FullName,
                RoomFee = i.RoomFee,
                ElectricFee = i.ElectricFee,
                WaterFee = i.WaterFee,
                TotalAmount = i.TotalAmount,
                Status = i.Status,
                IssuedAt = i.IssuedAt
            }).ToList()
        };

        return (true, "Lấy thống kê doanh thu thành công.", result);
    }

    public async Task<byte[]> ExportToExcelAsync(RevenueFilterDto filter)
    {
        var (success, _, data) = await GetRevenueAsync(filter);
        if (!success || data == null || !data.Details.Any())
            return Array.Empty<byte>();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();

        var worksheet = package.Workbook.Worksheets.Add("RevenueReport");

        // Headers
        worksheet.Cells[1, 1].Value = "Kỳ thu";
        worksheet.Cells[1, 2].Value = "Mã phòng";
        worksheet.Cells[1, 3].Value = "Sinh viên";
        worksheet.Cells[1, 4].Value = "Tiền phòng";
        worksheet.Cells[1, 5].Value = "Tiền điện";
        worksheet.Cells[1, 6].Value = "Tiền nước";
        worksheet.Cells[1, 7].Value = "Tổng cộng";
        worksheet.Cells[1, 8].Value = "Trạng thái";
        worksheet.Cells[1, 9].Value = "Ngày tạo";

        using (var range = worksheet.Cells[1, 1, 1, 9])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
        }

        int row = 2;
        foreach (var item in data.Details)
        {
            worksheet.Cells[row, 1].Value = item.Period;
            worksheet.Cells[row, 2].Value = item.RoomCode;
            worksheet.Cells[row, 3].Value = item.StudentName;
            worksheet.Cells[row, 4].Value = item.RoomFee;
            worksheet.Cells[row, 5].Value = item.ElectricFee;
            worksheet.Cells[row, 6].Value = item.WaterFee;
            worksheet.Cells[row, 7].Value = item.TotalAmount;
            worksheet.Cells[row, 8].Value = item.Status == "Paid" ? "Đã thanh toán" : "Chưa thanh toán";
            worksheet.Cells[row, 9].Value = item.IssuedAt.ToString("dd/MM/yyyy HH:mm:ss");

            row++;
        }

        // Summary Row
        worksheet.Cells[row, 3].Value = "TỔNG CỘNG";
        worksheet.Cells[row, 3].Style.Font.Bold = true;
        worksheet.Cells[row, 4].Value = data.TotalRoomFee;
        worksheet.Cells[row, 5].Value = data.TotalElectricFee;
        worksheet.Cells[row, 6].Value = data.TotalWaterFee;
        worksheet.Cells[row, 7].Value = data.GrandTotal;

        using (var summaryRange = worksheet.Cells[row, 3, row, 7])
        {
            summaryRange.Style.Font.Bold = true;
            summaryRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            summaryRange.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
        }

        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }
}