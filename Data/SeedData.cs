using BackendAPI.Models.Entities;
using BCrypt.Net;
namespace BackendAPI.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // Chỉ seed nếu chưa có data
        if (context.Users.Any()) return;

        // 1. Tạo tài khoản Admin & User mẫu
        var adminUser = new User
        {
            Email = "admin@ktx.edu.vn",
            FullName = "Ban Quản Lý KTX",
            Phone = "0999111222",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = "Admin",
            IsActive = true
        };

        var studentUser = new User
        {
            Email = "sinhvien@ktx.edu.vn",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student@123"),
            Role = "Student",
            IsActive = true
        };

        context.Users.AddRange(adminUser, studentUser);
        await context.SaveChangesAsync();

        // 2. Tạo thông tin sinh viên liên kết với User
        var student = new Student
        {
            UserId = studentUser.Id,
            FullName = "Nguyễn Văn A",
            CitizenId = "051206004478",
            Gender = "Nam",
            Phone = "0338055106",
            Email = "sinhvien@ktx.edu.vn",
            PermanentAddress = "123 Đường ABC, Đà Nẵng"
        };
        context.Students.Add(student);
        await context.SaveChangesAsync();

        var relativeA = new Relative
        {
            StudentId = student.Id,
            FullName = "Nguyễn Văn B",
            Phone = "0912345678",
            Relationship = "Bố"
        };
        context.Relatives.Add(relativeA);
        await context.SaveChangesAsync();

        // 3. Tạo Tòa nhà & Phòng KTX
        var buildingA = new Building
        {
            Code = "A",
            Name = "Tòa A",
            GenderAllowed = "Nam"
        };
        var buildingB = new Building
        {
            Code = "B",
            Name = "Tòa B",
            GenderAllowed = "Nữ"
        };
        context.Buildings.AddRange(buildingA, buildingB);
        await context.SaveChangesAsync();

        var roomA101 = new Room { BuildingId = buildingA.Id, RoomCode = "A101", RoomType = "6 người", Capacity = 6, CurrentOccupancy = 1, Status = "Available" };
        var roomA102 = new Room { BuildingId = buildingA.Id, RoomCode = "A102", RoomType = "4 người", Capacity = 4, CurrentOccupancy = 4, Status = "Full" };
        var roomA103 = new Room { BuildingId = buildingA.Id, RoomCode = "A103", RoomType = "6 người", Capacity = 6, CurrentOccupancy = 3, Status = "Available" };
        var roomB101 = new Room { BuildingId = buildingB.Id, RoomCode = "B101", RoomType = "4 người", Capacity = 4, CurrentOccupancy = 0, Status = "Available" };
        
        context.Rooms.AddRange(roomA101, roomA102, roomA103, roomB101);
        await context.SaveChangesAsync();

        // 4. Tạo gói gia hạn (Renewal Packages) & Học kỳ (Semester Periods)
        var package1 = new RenewalPackages { Name = "1 Kỳ", DurationMonths = 5, IsActive = true };
        var package2 = new RenewalPackages { Name = "1 Năm", DurationMonths = 10, IsActive = true };
        context.RenewalPackages.AddRange(package1, package2);

        var semester1 = new SemesterPeriods 
        { 
            Name = "HK1 2025-2026", 
            StartDate = new DateTime(2025, 8, 15), 
            EndDate = new DateTime(2026, 1, 15), 
            IsRegistrationOpen = true 
        };
        context.SemesterPeriods.Add(semester1);
        await context.SaveChangesAsync();

        // 5. Tạo một Đơn đăng ký chờ duyệt (từ Guest chưa có tài khoản)
        var pendingReg = new Registration
        {
            RegistrationCode = "REG_20260101_ABCDEF",
            RoomId = roomA101.Id,
            FullName = "Trần Văn B",
            CitizenId = "048201002345",
            Gender = "Nam",
            Phone = "0987654321",
            Email = "studentB@gmail.com",
            PermanentAddress = "456 Đường XYZ, Hà Nội",
            RelativeName = "Trần Văn C",
            RelativePhone = "0999888777",
            Relationship = "Mẹ",
            StartDate = semester1.StartDate,
            EndDate = semester1.EndDate,
            Status = "Pending"
        };
        context.Registrations.Add(pendingReg);

        // 6. Tạo Đơn đăng ký đã duyệt cho Nguyễn Văn A (và Hợp đồng)
        var approvedReg = new Registration
        {
            RegistrationCode = "REG_20260101_123456",
            StudentId = student.Id,
            RoomId = roomA101.Id,
            FullName = student.FullName,
            CitizenId = student.CitizenId,
            Gender = student.Gender,
            Phone = student.Phone,
            Email = student.Email,
            PermanentAddress = student.PermanentAddress,
            RelativeName = relativeA.FullName,
            RelativePhone = relativeA.Phone,
            Relationship = relativeA.Relationship,
            StartDate = semester1.StartDate,
            EndDate = semester1.EndDate,
            Status = "Approved"
        };
        context.Registrations.Add(approvedReg);
        await context.SaveChangesAsync();

        // Tạo hợp đồng tương ứng với đơn duyệt
        var contract = new Contract
        {
            ContractCode = "HD_20260101_123456",
            StudentId = student.Id,
            RoomId = roomA101.Id,
            StartDate = semester1.StartDate,
            EndDate = semester1.EndDate,
            Status = "Active",
            Price = 350000 // Giá phòng 6 người
        };
        context.Contracts.Add(contract);

        // 7. Tạo Hóa đơn (Invoice)
        var invoice = new Invoice
        {
            StudentId = student.Id,
            RoomId = roomA101.Id,
            Period = "09/2025",
            RoomFee = 350000,
            ElectricFee = 50000,
            WaterFee = 20000,
            TotalAmount = 420000,
            Status = "Unpaid"
        };
        context.Invoices.Add(invoice);

        // 8. Tạo dữ liệu Điện Nước (ElectricWaterReading) cho phòng A101
        var reading = new ElectricWaterReading
        {
            RoomId = roomA101.Id,
            Period = "09/2025",
            OldElectric = 100,
            NewElectric = 150,
            OldWater = 10,
            NewWater = 15
        };
        context.ElectricWaterReadings.Add(reading);

        // 9. Tạo Vi phạm (Violation Record)
        var violation = new ViolationRecord
        {
            StudentId = student.Id,
            ViolationType = "Vệ sinh",
            Description = "Không dọn rác khu vực hành lang",
            ViolationDate = DateTime.UtcNow.AddDays(-5),
            TotalCount = 1
        };
        context.ViolationRecords.Add(violation);

        // 10. Tạo Thông báo (Notification)
        var notification = new Notification
        {
            UserId = adminUser.Id,
            Title = "Có đơn đăng ký mới",
            Message = "Sinh viên Trần Văn B vừa nộp đơn đăng ký phòng A101",
            IsRead = false
        };
        context.Notifications.Add(notification);

        await context.SaveChangesAsync();
    }
}