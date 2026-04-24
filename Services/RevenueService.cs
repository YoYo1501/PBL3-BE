using BackendAPI.Exceptions;
using BackendAPI.Models.DTOs.Common;
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
        var validationMessage = ValidateFilter(filter);
        if (validationMessage != null)
            return (false, validationMessage, null);

        var (startDate, endDate) = NormalizeDateRange(filter);
        var invoices = await repo.GetInvoicesAsync(
            startDate, endDate,
            filter.RoomCode, filter.Period);

        if (!invoices.Any())
            return (false, "Không có dữ liệu doanh thu trong khoảng thời gian này.", null);

        var details = invoices.Select(i => new RevenueDetailDto
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
        }).ToList();

        var page = filter.GetPage();
        var pageSize = filter.GetPageSize(8);
        var totalItems = details.Count;
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalItems / (double)pageSize));
        if (page > totalPages) page = totalPages;

        var pagedDetails = details
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var result = new RevenueResponseDto
        {
            TotalRoomFee = invoices.Sum(i => i.RoomFee),
            TotalElectricFee = invoices.Sum(i => i.ElectricFee),
            TotalWaterFee = invoices.Sum(i => i.WaterFee),
            GrandTotal = invoices.Sum(i => i.TotalAmount),
            TotalInvoices = invoices.Count,
            PaidInvoices = invoices.Count(i => i.Status == "Paid"),
            UnpaidInvoices = invoices.Count(i => i.Status == "Unpaid"),
            Details = new PagedResultDto<RevenueDetailDto>
            {
                Items = pagedDetails,
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            }
        };

        return (true, "Lấy thống kê doanh thu thành công.", result);
    }

    public async Task<byte[]> ExportToExcelAsync(RevenueFilterDto filter)
    {
        var validationMessage = ValidateFilter(filter);
        if (validationMessage != null)
            throw new BadRequestException(validationMessage);

        var (startDate, endDate) = NormalizeDateRange(filter);
        var invoices = await repo.GetInvoicesAsync(
            startDate,
            endDate,
            filter.RoomCode,
            filter.Period);

        if (!invoices.Any())
            return Array.Empty<byte>();

        var details = invoices.Select(i => new RevenueDetailDto
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
        }).ToList();

        var totalRoomFee = invoices.Sum(i => i.RoomFee);
        var totalElectricFee = invoices.Sum(i => i.ElectricFee);
        var totalWaterFee = invoices.Sum(i => i.WaterFee);
        var grandTotal = invoices.Sum(i => i.TotalAmount);

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();

        var worksheet = package.Workbook.Worksheets.Add("RevenueReport");

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
        foreach (var item in details)
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

        worksheet.Cells[row, 3].Value = "TỔNG CỘNG";
        worksheet.Cells[row, 3].Style.Font.Bold = true;
        worksheet.Cells[row, 4].Value = totalRoomFee;
        worksheet.Cells[row, 5].Value = totalElectricFee;
        worksheet.Cells[row, 6].Value = totalWaterFee;
        worksheet.Cells[row, 7].Value = grandTotal;

        using (var summaryRange = worksheet.Cells[row, 3, row, 7])
        {
            summaryRange.Style.Font.Bold = true;
            summaryRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            summaryRange.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
        }

        worksheet.Cells.AutoFitColumns();

        return package.GetAsByteArray();
    }

    private static (DateTime StartDate, DateTime EndDate) NormalizeDateRange(RevenueFilterDto filter)
        => (filter.StartDate.Date, filter.EndDate.Date.AddDays(1).AddTicks(-1));

    private static string? ValidateFilter(RevenueFilterDto filter)
    {
        if (filter.StartDate.Date > filter.EndDate.Date)
            return "Ngày bắt đầu không được lớn hơn ngày kết thúc.";

        if (filter.EndDate.Date > DateTime.Today)
            return "Ngày kết thúc không được vượt quá ngày hiện tại.";

        return null;
    }
}
