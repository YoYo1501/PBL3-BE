using BackendAPI.Models.DTOs.Common;
using BackendAPI.Models.DTOs.Invoice.Requests;
using BackendAPI.Models.DTOs.Invoice.Responses;
using BackendAPI.Models.Entities;
using BackendAPI.Helpers;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System.Text.RegularExpressions;

namespace BackendAPI.Services;

public class InvoiceService(
    IInvoiceRepository repo,
    INotificationService notificationService,
    IReceiptRepository receiptRepository) : IInvoiceService
{
    public async Task<(bool Success, string Message, List<ImportResultDto>? Preview)> ImportExcelAsync(IFormFile file, string period)
    {
        period = NormalizePeriod(period);

        if (file == null || file.Length == 0)
            return (false, "Vui lòng chọn file Excel.", null);

        if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            return (false, "Chỉ chấp nhận file .xlsx.", null);

        if (!IsValidPeriod(period))
            return (false, "Kỳ hóa đơn không hợp lệ. Vui lòng dùng định dạng yyyy-MM.", null);

        var results = new List<ImportResultDto>();
        var errors = new List<string>();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        using var package = new ExcelPackage(stream);

        var sheet = package.Workbook.Worksheets[0];
        var rowCount = sheet.Dimension?.Rows ?? 0;

        if (rowCount < 2)
            return (false, "File Excel không có dữ liệu.", null);

        for (int row = 2; row <= rowCount; row++)
        {
            var roomCode = sheet.Cells[row, 1].Text.Trim();
            if (string.IsNullOrEmpty(roomCode)) continue;

            if (!decimal.TryParse(sheet.Cells[row, 2].Text, out var oldElectric) ||
                !decimal.TryParse(sheet.Cells[row, 3].Text, out var newElectric) ||
                !decimal.TryParse(sheet.Cells[row, 4].Text, out var oldWater) ||
                !decimal.TryParse(sheet.Cells[row, 5].Text, out var newWater))
            {
                errors.Add($"Dòng {row}: Dữ liệu không hợp lệ (phòng {roomCode}).");
                continue;
            }

            if (newElectric < oldElectric || newWater < oldWater)
            {
                errors.Add($"Dòng {row}: Chỉ số mới phải lớn hơn hoặc bằng chỉ số cũ (phòng {roomCode}).");
                continue;
            }

            var room = await repo.GetRoomByCodeAsync(roomCode);
            if (room == null)
            {
                errors.Add($"Dòng {row}: Phòng {roomCode} không tồn tại.");
                continue;
            }

            var alreadyImported = await repo.PeriodAlreadyImportedAsync(room.Id, period);
            if (alreadyImported)
            {
                errors.Add($"Dòng {row}: Phòng {roomCode} đã import cho kỳ {period}.");
                continue;
            }

            await repo.AddReadingAsync(new ElectricWaterReading
            {
                RoomId = room.Id,
                Period = period,
                OldElectric = oldElectric,
                NewElectric = newElectric,
                OldWater = oldWater,
                NewWater = newWater
            });

            results.Add(new ImportResultDto
            {
                RoomCode = roomCode,
                OldElectric = oldElectric,
                NewElectric = newElectric,
                OldWater = oldWater,
                NewWater = newWater
            });
        }

        if (!results.Any() && errors.Any())
            return (false, string.Join("\n", errors), null);

        await repo.SaveChangesAsync();

        if (errors.Any())
            return (false, string.Join("\n", errors), results);

        return (true, $"Import thành công {results.Count} phòng.", results);
    }

    public async Task<(bool Success, string Message, List<InvoiceDraftDto>? Drafts)> GenerateInvoicesAsync(InvoiceSettingDto dto)
    {
        dto.Period = NormalizePeriod(dto.Period);

        if (string.IsNullOrEmpty(dto.Period))
            return (false, "Vui lòng nhập kỳ hóa đơn.", null);

        if (!IsValidPeriod(dto.Period))
            return (false, "Kỳ hóa đơn không hợp lệ. Vui lòng dùng định dạng yyyy-MM.", null);

        if (dto.ElectricPricePerKwh <= 0 || dto.WaterPricePerM3 <= 0)
            return (false, "Giá điện và giá nước phải lớn hơn 0.", null);

        if (dto.DueDay < 1 || dto.DueDay > 31)
            return (false, "Ngay han thanh toan phai nam trong khoang 1 den 31.", null);

        var readings = await repo.GetReadingsByPeriodAsync(dto.Period);
        if (!readings.Any())
            return (false, $"Không có dữ liệu điện nước cho kỳ {dto.Period}.", null);

        var drafts = new List<InvoiceDraftDto>();
        var skippedExistingInvoices = 0;
        var dueDate = CalculateDueDate(dto.Period, dto.DueDay);

        foreach (var reading in readings)
        {
            var contracts = await repo.GetActiveContractsByRoomAsync(reading.RoomId);
            if (!contracts.Any()) continue;

            var studentCount = contracts.Count;
            var electricUsed = reading.NewElectric - reading.OldElectric;
            var waterUsed = reading.NewWater - reading.OldWater;
            var totalElectricFee = electricUsed * dto.ElectricPricePerKwh;
            var totalWaterFee = waterUsed * dto.WaterPricePerM3;

            foreach (var contract in contracts)
            {
                if (await repo.InvoiceExistsAsync(contract.StudentId, reading.RoomId, dto.Period))
                {
                    skippedExistingInvoices++;
                    continue;
                }

                var electricFee = totalElectricFee / studentCount;
                var waterFee = totalWaterFee / studentCount;
                var roomFee = contract.Price;
                var total = roomFee + electricFee + waterFee;

                var invoice = new Invoice
                {
                    StudentId = contract.StudentId,
                    RoomId = reading.RoomId,
                    Period = dto.Period,
                    RoomFee = roomFee,
                    ElectricFee = electricFee,
                    WaterFee = waterFee,
                    TotalAmount = total,
                    Status = "Draft",
                    DueDate = dueDate
                };

                await repo.AddInvoiceAsync(invoice);
                drafts.Add(new InvoiceDraftDto
                {
                    StudentName = contract.Student.FullName,
                    RoomCode = reading.Room.RoomCode,
                    Period = dto.Period,
                    RoomFee = roomFee,
                    ElectricFee = electricFee,
                    WaterFee = waterFee,
                    TotalAmount = total,
                    Status = "Draft",
                    DueDate = dueDate
                });
            }
        }

        if (!drafts.Any())
        {
            if (skippedExistingInvoices > 0)
                return (false, $"Kỳ {dto.Period} đã có hóa đơn, hệ thống không tạo thêm để tránh trùng lặp.", null);

            return (false, $"Không có hợp đồng hiệu lực để tạo hóa đơn cho kỳ {dto.Period}.", null);
        }

        await repo.SaveChangesAsync();

        var message = $"Tạo {drafts.Count} hóa đơn dự thảo thành công.";
        if (skippedExistingInvoices > 0)
            message += $" Bỏ qua {skippedExistingInvoices} hóa đơn đã tồn tại.";

        return (true, message, drafts);
    }

    public async Task<List<InvoiceDraftDto>> GetDraftInvoicesAsync(string period)
    {
        period = NormalizePeriod(period);
        if (!IsValidPeriod(period))
            return [];

        var list = await repo.GetDraftInvoicesAsync(period);
        return list.Select(ToDto).ToList();
    }

    public async Task<(bool Success, string Message)> PublishInvoicesAsync(string period)
    {
        period = NormalizePeriod(period);
        if (!IsValidPeriod(period))
            return (false, "Kỳ hóa đơn không hợp lệ. Vui lòng dùng định dạng yyyy-MM.");

        var drafts = await repo.GetDraftInvoicesAsync(period);
        if (!drafts.Any())
            return (false, $"Không có hóa đơn dự thảo cho kỳ {period}.");

        foreach (var invoice in drafts)
        {
            invoice.Status = "Unpaid";
            invoice.IssuedAt = DateTime.UtcNow;
            invoice.DueDate ??= CalculateDueDate(invoice.Period, 15);
            await repo.UpdateInvoiceAsync(invoice);
        }

        await repo.SaveChangesAsync();
        return (true, $"Phát hành {drafts.Count} hóa đơn thành công.");
    }

    public async Task<List<InvoiceDraftDto>> GetMyInvoicesAsync(int studentId)
    {
        var list = await repo.GetMyInvoicesAsync(studentId);
        return list.Select(i => new InvoiceDraftDto
        {
            Id = i.Id,
            InvoiceCode = BuildInvoiceCode(i),
            StudentName = i.Student?.FullName ?? string.Empty,
            RoomCode = i.Room.RoomCode,
            Period = i.Period,
            RoomFee = i.RoomFee,
            ElectricFee = i.ElectricFee,
            WaterFee = i.WaterFee,
            TotalAmount = i.TotalAmount,
            Status = i.Status,
            IssuedAt = VietnamTime.FromUtc(i.IssuedAt),
            DueDate = i.DueDate,
            PaidAt = VietnamTime.FromUtc(i.PaidAt),
            PaymentMethod = i.PaymentMethod,
            TransactionCode = i.TransactionCode
        }).ToList();
    }

    public async Task<byte[]> ExportInvoicesAsync(string period)
    {
        period = NormalizePeriod(period);
        if (!IsValidPeriod(period))
            return Array.Empty<byte>();

        var invoices = await repo.GetAllInvoicesAsync(period, null);
        if (!invoices.Any())
            return Array.Empty<byte>();

        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("HoaDon");

        worksheet.Cells[1, 1].Value = "Kỳ";
        worksheet.Cells[1, 2].Value = "Phòng";
        worksheet.Cells[1, 3].Value = "Sinh viên";
        worksheet.Cells[1, 4].Value = "Tiền phòng";
        worksheet.Cells[1, 5].Value = "Tiền điện";
        worksheet.Cells[1, 6].Value = "Tiền nước";
        worksheet.Cells[1, 7].Value = "Tổng cộng";
        worksheet.Cells[1, 8].Value = "Trạng thái";
        worksheet.Cells[1, 9].Value = "Ngày tạo";

        worksheet.Cells[1, 10].Value = "Han thanh toan";

        using (var range = worksheet.Cells[1, 1, 1, 10])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        var row = 2;
        foreach (var invoice in invoices)
        {
            worksheet.Cells[row, 1].Value = invoice.Period;
            worksheet.Cells[row, 2].Value = invoice.Room?.RoomCode ?? "";
            worksheet.Cells[row, 3].Value = invoice.Student?.FullName ?? "";
            worksheet.Cells[row, 4].Value = invoice.RoomFee;
            worksheet.Cells[row, 5].Value = invoice.ElectricFee;
            worksheet.Cells[row, 6].Value = invoice.WaterFee;
            worksheet.Cells[row, 7].Value = invoice.TotalAmount;
            worksheet.Cells[row, 8].Value = GetInvoiceStatusLabel(invoice.Status);
            worksheet.Cells[row, 9].Value = invoice.IssuedAt.ToString("dd/MM/yyyy HH:mm:ss");
            worksheet.Cells[row, 10].Value = (invoice.DueDate ?? CalculateDueDate(invoice.Period, 15)).ToString("dd/MM/yyyy");
            row++;
        }

        worksheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public async Task<List<InvoiceDraftDto>> GetAllInvoicesAsync(string? period, string? status)
    {
        period = NormalizePeriod(period);
        var invoices = await repo.GetAllInvoicesAsync(period, status);
        return invoices.Select(ToDto).ToList();
    }

    public async Task<PagedResultDto<InvoiceDraftDto>> GetPagedInvoicesAsync(InvoiceListQueryDto query)
    {
        query.Period = NormalizePeriod(query.Period);

        var page = query.GetPage();
        var pageSize = query.GetPageSize();
        var (items, totalCount) = await repo.GetPagedInvoicesAsync(query.Period, query.Status, page, pageSize);

        return new PagedResultDto<InvoiceDraftDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalItems = totalCount,
            TotalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize))
        };
    }

    public async Task<InvoiceDraftDto?> GetInvoiceByIdAsync(int id)
    {
        var invoice = await repo.GetInvoiceByIdAsync(id);
        return invoice == null ? null : ToDto(invoice);
    }

    public async Task<(bool Success, string Message)> PayInvoiceManuallyAsync(int id)
    {
        var invoice = await repo.GetInvoiceByIdAsync(id);
        if (invoice == null)
            return (false, "Hóa đơn không tồn tại.");

        if (invoice.Status == "Paid")
            return (false, "Hóa đơn đã được thanh toán.");

        if (invoice.Status != "Unpaid")
            return (false, "Chỉ có thể thu tiền hóa đơn đã phát hành và chưa thanh toán.");

        var paidAt = DateTime.UtcNow;
        invoice.Status = "Paid";
        invoice.PaidAt = paidAt;
        invoice.PaymentMethod = "Cash";
        invoice.TransactionCode = $"CASH-{invoice.Id}-{paidAt:yyyyMMddHHmmss}";
        await repo.UpdateInvoiceAsync(invoice);

        if (!await receiptRepository.ReceiptExistsByInvoiceIdAsync(invoice.Id))
        {
            var period = invoice.Period
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .Replace("/", string.Empty, StringComparison.Ordinal)
                .Replace(" ", string.Empty, StringComparison.Ordinal);

            await receiptRepository.AddReceiptAsync(new Receipt
            {
                InvoiceId = invoice.Id,
                ReceiptCode = $"BL-{period}-{invoice.Id:D6}",
                PaidAmount = invoice.TotalAmount,
                PaidAt = paidAt,
                PaymentMethod = invoice.PaymentMethod ?? "Cash",
                TransactionCode = invoice.TransactionCode ?? string.Empty,
                Status = "Success"
            });
        }

        await receiptRepository.SaveChangesAsync();

        return (true, "Thanh toán hóa đơn bằng tiền mặt thành công.");
    }

    public async Task<(bool Success, string Message)> RemindDebtAsync(string? period)
    {
        period = NormalizePeriod(period);
        if (!string.IsNullOrEmpty(period) && !IsValidPeriod(period))
            return (false, "Kỳ hóa đơn không hợp lệ. Vui lòng dùng định dạng yyyy-MM.");

        var invoices = await repo.GetAllInvoicesAsync(period, "Unpaid");
        if (!invoices.Any())
            return (false, "Không có hóa đơn nào chưa thanh toán để nhắc nợ.");

        var count = 0;
        foreach (var i in invoices)
        {
            if (i.Student?.UserId != null)
            {
                await notificationService.CreateAsync(new BackendAPI.Models.DTOs.Notification.Requests.CreateNotificationDto
                {
                    UserId = i.Student.UserId,
                    Title = "Thông báo nhắc nợ hóa đơn",
                    Message = $"Bạn có hóa đơn chưa thanh toán cho kỳ {i.Period}. Tổng số tiền là {i.TotalAmount:N0} VND. Vui lòng thanh toán sớm!"
                });
                count++;
            }
        }

        return (true, $"Đã gửi thông báo nhắc nợ thành công cho {count} sinh viên.");
    }

    public async Task<(bool Success, string Message)> RemindUpcomingDueAsync(int beforeDueDays = 3)
    {
        if (beforeDueDays < 0 || beforeDueDays > 30)
            return (false, "So ngay nhac truoc han phai nam trong khoang 0 den 30.");

        var today = DateTime.UtcNow.Date;
        var toDate = today.AddDays(beforeDueDays).AddDays(1).AddTicks(-1);
        var invoices = await repo.GetUnpaidInvoicesDueBetweenAsync(today, toDate);

        if (!invoices.Any())
            return (false, "Khong co hoa don nao sap den han can nhac.");

        var count = 0;
        foreach (var invoice in invoices)
        {
            if (invoice.Student?.UserId == null || invoice.DueDate == null)
                continue;

            var daysLeft = Math.Max(0, (invoice.DueDate.Value.Date - today).Days);
            await notificationService.CreateAsync(new BackendAPI.Models.DTOs.Notification.Requests.CreateNotificationDto
            {
                UserId = invoice.Student.UserId,
                Title = "Thong bao hoa don sap den han",
                Message = $"Hoa don ky {invoice.Period} se den han vao ngay {invoice.DueDate:dd/MM/yyyy} (con {daysLeft} ngay). Tong so tien la {invoice.TotalAmount:N0} VND. Vui long thanh toan dung han."
            });
            count++;
        }

        return (true, $"Da gui thong bao sap den han cho {count} sinh vien.");
    }

    private static InvoiceDraftDto ToDto(Invoice i) => new()
    {
        Id = i.Id,
        InvoiceCode = BuildInvoiceCode(i),
        StudentName = i.Student?.FullName ?? "",
        RoomCode = i.Room?.RoomCode ?? "",
        Period = i.Period,
        RoomFee = i.RoomFee,
        ElectricFee = i.ElectricFee,
        WaterFee = i.WaterFee,
        TotalAmount = i.TotalAmount,
        Status = i.Status,
        IssuedAt = VietnamTime.FromUtc(i.IssuedAt),
        DueDate = i.DueDate,
        PaidAt = VietnamTime.FromUtc(i.PaidAt),
        PaymentMethod = i.PaymentMethod,
        TransactionCode = i.TransactionCode
    };

    private static string NormalizePeriod(string? period)
        => string.IsNullOrWhiteSpace(period) ? string.Empty : period.Trim();

    private static bool IsValidPeriod(string period)
        => Regex.IsMatch(period, @"^\d{4}-(0[1-9]|1[0-2])$");

    private static string BuildInvoiceCode(Invoice invoice)
    {
        var period = invoice.Period;
        var yearMonth = Regex.Match(period, @"^(?<year>\d{4})[-/](?<month>\d{1,2})$");
        if (yearMonth.Success)
        {
            period = $"{yearMonth.Groups["year"].Value}{int.Parse(yearMonth.Groups["month"].Value):D2}";
        }
        else
        {
            var monthYear = Regex.Match(period, @"^(?<month>\d{1,2})[-/](?<year>\d{4})$");
            period = monthYear.Success
                ? $"{monthYear.Groups["year"].Value}{int.Parse(monthYear.Groups["month"].Value):D2}"
                : period
                    .Replace("-", string.Empty, StringComparison.Ordinal)
                    .Replace("/", string.Empty, StringComparison.Ordinal)
                    .Replace(" ", string.Empty, StringComparison.Ordinal);
        }

        if (string.IsNullOrWhiteSpace(period))
            period = invoice.IssuedAt.ToString("yyyyMM");

        return $"HD-{period}-{invoice.Id:D6}";
    }

    private static DateTime CalculateDueDate(string period, int dueDay)
    {
        var year = int.Parse(period[..4]);
        var month = int.Parse(period[5..7]);
        var maxDay = DateTime.DaysInMonth(year, month);
        var day = Math.Clamp(dueDay, 1, maxDay);
        return new DateTime(year, month, day, 23, 59, 59, DateTimeKind.Utc);
    }

    private static string GetInvoiceStatusLabel(string? status) => status switch
    {
        "Paid" => "Đã thanh toán",
        "Unpaid" => "Chưa thanh toán",
        "Draft" => "Nháp",
        _ => status ?? string.Empty
    };
}
