using BackendAPI.Models.Entities;

namespace BackendAPI.Data;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext context)
    {
        await BackfillRoomPricesAsync(context);

        if (context.Users.Any()) return;

        var now = DateTime.UtcNow;

        var adminUsers = CreateAdminUsers();
        context.Users.AddRange(adminUsers);

        var buildings = CreateBuildings();
        context.Buildings.AddRange(buildings.Values);

        var rooms = CreateRooms(buildings);
        context.Rooms.AddRange(rooms.Values);

        var semesters = CreateSemesters(now);
        context.SemesterPeriods.AddRange(semesters.Values);

        var renewalPackages = CreateRenewalPackages();
        context.RenewalPackages.AddRange(renewalPackages.Values);

        var students = CreateStudents(now);
        context.Users.AddRange(students.Select(s => s.User));
        context.Students.AddRange(students.Select(s => s.Student));
        context.Relatives.AddRange(students.Select(s => s.Relative));

        var contracts = CreateContracts(now, students, rooms);
        ApplyRoomOccupancy(rooms.Values, contracts, now);
        context.Contracts.AddRange(contracts);

        var registrations = CreateRegistrations(now, students, rooms, semesters);
        context.Registrations.AddRange(registrations);

        var invoices = CreateInvoices(now, students, rooms, contracts);
        context.Invoices.AddRange(invoices);

        var readings = CreateElectricWaterReadings(now, rooms);
        context.ElectricWaterReadings.AddRange(readings);

        var violations = CreateViolationRecords(now, students);
        context.ViolationRecords.AddRange(violations);

        var studentRequests = CreateStudentRequests(now, students);
        context.StudentRequests.AddRange(studentRequests);

        var transferRequests = CreateRoomTransferRequests(now, students, rooms, semesters);
        context.RoomTransferRequests.AddRange(transferRequests);

        var renewalRequests = CreateRenewalRequests(now, students, contracts, renewalPackages);
        context.RenewalRequests.AddRange(renewalRequests);

        var notifications = CreateNotifications(now, adminUsers, students);
        context.Notifications.AddRange(notifications);

        var facilities = CreateFacilities(now, rooms);
        context.Facilities.AddRange(facilities);

        await context.SaveChangesAsync();
    }

    private static List<User> CreateAdminUsers()
    {
        return
        [
            new User
            {
                CitizenId = "000000000000",
                FullName = "Ban Quan Ly KTX",
                Phone = "0999111222",
                Email = "admin.ktx@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                IsActive = true
            },
            new User
            {
                CitizenId = "048200000001",
                FullName = "Sanh",
                Phone = "0999111223",
                Email = "sanh.admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                IsActive = true
            },
            new User
            {
                CitizenId = "048200000002",
                FullName = "Linh",
                Phone = "0999111224",
                Email = "linh.admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                IsActive = true
            },
            new User
            {
                CitizenId = "048200000003",
                FullName = "Viet",
                Phone = "0999111225",
                Email = "viet.admin@example.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = "Admin",
                IsActive = true
            }
        ];
    }

    private static Dictionary<string, Building> CreateBuildings()
    {
        return new Dictionary<string, Building>
        {
            ["A"] = new() { Code = "A", Name = "Toa A", GenderAllowed = "Nam" },
            ["B"] = new() { Code = "B", Name = "Toa B", GenderAllowed = "Nữ" },
            ["C"] = new() { Code = "C", Name = "Toa C", GenderAllowed = "Nam" },
            ["D"] = new() { Code = "D", Name = "Toa D", GenderAllowed = "Nữ" }
        };
    }

    private static Dictionary<string, Room> CreateRooms(IReadOnlyDictionary<string, Building> buildings)
    {
        return new Dictionary<string, Room>
        {
            ["A101"] = NewRoom(buildings["A"], "A101", 6, false),
            ["A102"] = NewRoom(buildings["A"], "A102", 4, false),
            ["A103"] = NewRoom(buildings["A"], "A103", 8, false),
            ["A104"] = NewRoom(buildings["A"], "A104", 6, true),
            ["C201"] = NewRoom(buildings["C"], "C201", 4, false),
            ["C202"] = NewRoom(buildings["C"], "C202", 8, false),
            ["B101"] = NewRoom(buildings["B"], "B101", 4, false),
            ["B102"] = NewRoom(buildings["B"], "B102", 8, false),
            ["B103"] = NewRoom(buildings["B"], "B103", 6, false),
            ["B104"] = NewRoom(buildings["B"], "B104", 4, true),
            ["D201"] = NewRoom(buildings["D"], "D201", 6, false),
            ["D202"] = NewRoom(buildings["D"], "D202", 8, false)
        };
    }

    private static Room NewRoom(Building building, string roomCode, int capacity, bool isLocked)
    {
        return new Room
        {
            Building = building,
            RoomCode = roomCode,
            RoomType = $"{capacity} người",
            Capacity = capacity,
            CurrentOccupancy = 0,
            Status = isLocked ? "Locked" : "Available",
            Price = GetDefaultRoomPrice(capacity)
        };
    }

    private static Dictionary<string, SemesterPeriods> CreateSemesters(DateTime now)
    {
        return new Dictionary<string, SemesterPeriods>
        {
            ["previous"] = new()
            {
                Name = "HK1 2025-2026",
                StartDate = now.AddMonths(-8),
                EndDate = now.AddMonths(-3),
                IsRegistrationOpen = false
            },
            ["current"] = new()
            {
                Name = "HK2 2025-2026",
                StartDate = now.AddMonths(-2),
                EndDate = now.AddMonths(4),
                IsRegistrationOpen = true
            },
            ["upcoming"] = new()
            {
                Name = "HK1 2026-2027",
                StartDate = now.AddMonths(5),
                EndDate = now.AddMonths(10),
                IsRegistrationOpen = false
            }
        };
    }

    private static Dictionary<string, RenewalPackages> CreateRenewalPackages()
    {
        return new Dictionary<string, RenewalPackages>
        {
            ["1ky"] = new() { Name = "1 Ky", DurationMonths = 5, IsActive = true },
            ["2ky"] = new() { Name = "2 Ky", DurationMonths = 10, IsActive = true },
            ["1nam"] = new() { Name = "1 Nam", DurationMonths = 12, IsActive = true }
        };
    }

    private static List<StudentSeed> CreateStudents(DateTime now)
    {
        return
        [
            NewStudent(now, "SV001", "Nguyen Van An", "051206004401", "Nam", "0338055101", "nguyenvanan.sv@example.com", "123 Le Duan, Da Nang", "Nguyen Van Ba", "0905000001", "Bo"),
            NewStudent(now, "SV002", "Tran Quoc Binh", "051206004402", "Nam", "0338055102", "tranquocbinh.sv@example.com", "45 Ong Ich Khiem, Da Nang", "Tran Thi Lan", "0905000002", "Me"),
            NewStudent(now, "SV003", "Le Minh Cuong", "051206004403", "Nam", "0338055103", "leminhcuong.sv@example.com", "12 Nguyen Tri Phuong, Hue", "Le Van Son", "0905000003", "Bo"),
            NewStudent(now, "SV004", "Pham Gia Duy", "051206004404", "Nam", "0338055104", "phamgiaduy.sv@example.com", "78 Tran Phu, Quang Nam", "Pham Thi Hoa", "0905000004", "Me"),
            NewStudent(now, "SV005", "Vo Thanh Hieu", "051206004405", "Nam", "0338055105", "vothanhhieu.sv@example.com", "90 Hai Ba Trung, Quang Ngai", "Vo Van Hanh", "0905000005", "Chu"),
            NewStudent(now, "SV006", "Do Duc Khanh", "051206004406", "Nam", "0338055106", "doduckhanh.sv@example.com", "15 Nguyen Van Linh, Da Nang", "Do Thi Tam", "0905000006", "Me"),
            NewStudent(now, "SV007", "Nguyen Thi Lan", "051206004407", "Nữ", "0338055107", "nguyenthilan.sv@example.com", "22 Hoang Dieu, Da Nang", "Nguyen Van Nho", "0905000007", "Bo"),
            NewStudent(now, "SV008", "Tran Thi My", "051206004408", "Nữ", "0338055108", "tranthimy.sv@example.com", "91 Hai Phong, Hue", "Tran Thi Nga", "0905000008", "Me"),
            NewStudent(now, "SV009", "Le Ngoc Thao", "051206004409", "Nữ", "0338055109", "lengocthao.sv@example.com", "27 Le Loi, Quang Tri", "Le Van Tuan", "0905000009", "Bo"),
            NewStudent(now, "SV010", "Pham Thu Uyen", "051206004410", "Nữ", "0338055110", "phamthuyen.sv@example.com", "66 Phan Chau Trinh, Da Nang", "Pham Thi Hanh", "0905000010", "Chi"),
            NewStudent(now, "SV011", "Vo Bao Vy", "051206004411", "Nữ", "0338055111", "vobaovy.sv@example.com", "11 Ly Thuong Kiet, Quang Nam", "Vo Van Dung", "0905000011", "Bo"),
            NewStudent(now, "SV012", "Hoang Gia Yen", "051206004412", "Nữ", "0338055112", "hoanggiayen.sv@example.com", "77 Hung Vuong, Da Nang", "Hoang Thi Mai", "0905000012", "Me")
        ];
    }

    private static StudentSeed NewStudent(
        DateTime now,
        string code,
        string fullName,
        string citizenId,
        string gender,
        string phone,
        string email,
        string address,
        string relativeName,
        string relativePhone,
        string relationship)
    {
        var user = new User
        {
            FullName = fullName,
            Phone = phone,
            Email = email,
            CitizenId = citizenId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(citizenId),
            Role = "Student",
            IsActive = true,
            MustChangePassword = true,
            CreatedAt = now.AddDays(-90)
        };

        var student = new Student
        {
            User = user,
            FullName = fullName,
            CitizenId = citizenId,
            Gender = gender,
            Phone = phone,
            Email = email,
            PermanentAddress = address,
            CreatedAt = now.AddDays(-90)
        };

        var relative = new Relative
        {
            Student = student,
            FullName = relativeName,
            Phone = relativePhone,
            Relationship = relationship
        };

        return new StudentSeed(code, user, student, relative);
    }

    private static List<Contract> CreateContracts(
        DateTime now,
        IReadOnlyList<StudentSeed> students,
        IReadOnlyDictionary<string, Room> rooms)
    {
        var map = students.ToDictionary(s => s.Code);

        return
        [
            NewContract("HD_20260101_0001", map["SV001"], rooms["A101"], now.AddMonths(-5), now.AddDays(20), "Active"),
            NewContract("HD_20260101_0002", map["SV002"], rooms["A101"], now.AddMonths(-4), now.AddDays(90), "Active"),
            NewContract("HD_20260101_0003", map["SV003"], rooms["A102"], now.AddMonths(-3), now.AddDays(120), "Active"),
            NewContract("HD_20260101_0004", map["SV004"], rooms["A103"], now.AddMonths(-2), now.AddDays(180), "Active"),
            NewContract("HD_20260101_0005", map["SV005"], rooms["C201"], now.AddMonths(-2), now.AddDays(45), "Active"),
            NewContract("HD_20251201_0006", map["SV006"], rooms["C202"], now.AddMonths(-10), now.AddMonths(-1), "Expired"),
            NewContract("HD_20260101_0007", map["SV007"], rooms["B101"], now.AddMonths(-5), now.AddDays(15), "Active"),
            NewContract("HD_20260101_0008", map["SV008"], rooms["B102"], now.AddMonths(-4), now.AddDays(80), "Active"),
            NewContract("HD_20260101_0009", map["SV009"], rooms["B103"], now.AddMonths(-3), now.AddDays(160), "Active"),
            NewContract("HD_20260101_0010", map["SV010"], rooms["D201"], now.AddMonths(-2), now.AddDays(220), "Active"),
            NewContract("HD_20251201_0011", map["SV011"], rooms["D202"], now.AddMonths(-11), now.AddMonths(-2), "Expired"),
            NewContract("HD_20260101_0012", map["SV012"], rooms["D202"], now.AddMonths(-1), now.AddDays(70), "Active")
        ];
    }

    private static Contract NewContract(string code, StudentSeed student, Room room, DateTime startDate, DateTime endDate, string status)
    {
        return new Contract
        {
            ContractCode = code,
            Student = student.Student,
            Room = room,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            Price = room.Price
        };
    }

    private static void ApplyRoomOccupancy(IEnumerable<Room> rooms, IEnumerable<Contract> contracts, DateTime now)
    {
        var activeContracts = contracts
            .Where(c => c.Status == "Active" && c.EndDate >= now)
            .GroupBy(c => c.Room)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (var room in rooms)
        {
            room.CurrentOccupancy = activeContracts.GetValueOrDefault(room);

            if (room.Status == "Locked")
                continue;

            room.Status = room.CurrentOccupancy >= room.Capacity ? "Full" : "Available";
        }
    }

    private static List<Registration> CreateRegistrations(
        DateTime now,
        IReadOnlyList<StudentSeed> students,
        IReadOnlyDictionary<string, Room> rooms,
        IReadOnlyDictionary<string, SemesterPeriods> semesters)
    {
        var map = students.ToDictionary(s => s.Code);
        var current = semesters["current"];
        var upcoming = semesters["upcoming"];

        return
        [
            NewApprovedRegistration("REG_20260101_0001", map["SV001"], rooms["A101"], current.StartDate, current.EndDate, now.AddMonths(-5)),
            NewApprovedRegistration("REG_20260101_0002", map["SV007"], rooms["B101"], current.StartDate, current.EndDate, now.AddMonths(-5)),
            NewApprovedRegistration("REG_20260101_0003", map["SV012"], rooms["D202"], current.StartDate, current.EndDate, now.AddMonths(-1)),
            NewPendingRegistration("REG_20260417_0004", "Bui Van Long", "048201002345", "Nam", "0987654321", "buivanlong.guest@example.com", "456 Duong XYZ, Ha Noi", "Bui Thi Hoa", "0999888777", "Me", rooms["A103"], upcoming.StartDate, upcoming.EndDate, now.AddDays(-3)),
            NewPendingRegistration("REG_20260417_0005", "Nguyen Thi Hanh", "048201002346", "Nữ", "0987654322", "nguyenthihanh.guest@example.com", "123 Tran Hung Dao, Quang Nam", "Nguyen Van Minh", "0999888778", "Bo", rooms["B102"], upcoming.StartDate, upcoming.EndDate, now.AddDays(-2)),
            NewRejectedRegistration("REG_20260417_0006", "Le Hoang Phuc", "048201002347", "Nam", "0987654323", "lehoangphuc.guest@example.com", "67 Ly Thai To, Da Nang", "Le Thi Thu", "0999888779", "Chi", rooms["A102"], upcoming.StartDate, upcoming.EndDate, "CCCD da ton tai trong he thong.", now.AddDays(-1))
        ];
    }

    private static Registration NewApprovedRegistration(string code, StudentSeed student, Room room, DateTime startDate, DateTime endDate, DateTime submittedAt)
    {
        return new Registration
        {
            RegistrationCode = code,
            Student = student.Student,
            Room = room,
            FullName = student.Student.FullName,
            CitizenId = student.Student.CitizenId,
            Gender = student.Student.Gender,
            Phone = student.Student.Phone,
            Email = student.Student.Email,
            PermanentAddress = student.Student.PermanentAddress,
            RelativeName = student.Relative.FullName,
            RelativePhone = student.Relative.Phone,
            Relationship = student.Relative.Relationship,
            StartDate = startDate,
            EndDate = endDate,
            Status = "Approved",
            SubmittedAt = submittedAt
        };
    }

    private static Registration NewPendingRegistration(
        string code,
        string fullName,
        string citizenId,
        string gender,
        string phone,
        string email,
        string address,
        string relativeName,
        string relativePhone,
        string relationship,
        Room room,
        DateTime startDate,
        DateTime endDate,
        DateTime submittedAt)
    {
        return new Registration
        {
            RegistrationCode = code,
            Room = room,
            FullName = fullName,
            CitizenId = citizenId,
            Gender = gender,
            Phone = phone,
            Email = email,
            PermanentAddress = address,
            RelativeName = relativeName,
            RelativePhone = relativePhone,
            Relationship = relationship,
            StartDate = startDate,
            EndDate = endDate,
            Status = "Pending",
            SubmittedAt = submittedAt
        };
    }

    private static Registration NewRejectedRegistration(
        string code,
        string fullName,
        string citizenId,
        string gender,
        string phone,
        string email,
        string address,
        string relativeName,
        string relativePhone,
        string relationship,
        Room room,
        DateTime startDate,
        DateTime endDate,
        string reason,
        DateTime submittedAt)
    {
        return new Registration
        {
            RegistrationCode = code,
            Room = room,
            FullName = fullName,
            CitizenId = citizenId,
            Gender = gender,
            Phone = phone,
            Email = email,
            PermanentAddress = address,
            RelativeName = relativeName,
            RelativePhone = relativePhone,
            Relationship = relationship,
            StartDate = startDate,
            EndDate = endDate,
            Status = "Rejected",
            RejectionReason = reason,
            SubmittedAt = submittedAt
        };
    }

    private static List<Invoice> CreateInvoices(
        DateTime now,
        IReadOnlyList<StudentSeed> students,
        IReadOnlyDictionary<string, Room> rooms,
        IReadOnlyList<Contract> contracts)
    {
        var studentMap = students.ToDictionary(s => s.Code);
        var contractMap = contracts.ToDictionary(c => c.Student.CitizenId);
        var periodCurrent = $"{now.Month:00}/{now.Year}";
        var previousDate = now.AddMonths(-1);
        var periodPrevious = $"{previousDate.Month:00}/{previousDate.Year}";

        return
        [
            NewInvoice(studentMap["SV001"], contractMap[studentMap["SV001"].Student.CitizenId].Room, periodPrevious, 52000, 18000, "Paid", now.AddDays(-30)),
            NewInvoice(studentMap["SV001"], contractMap[studentMap["SV001"].Student.CitizenId].Room, periodCurrent, 48000, 17000, "Paid", now.AddDays(-3)),
            NewInvoice(studentMap["SV002"], contractMap[studentMap["SV002"].Student.CitizenId].Room, periodCurrent, 55000, 20000, "Paid", now.AddDays(-3)),
            NewInvoice(studentMap["SV003"], contractMap[studentMap["SV003"].Student.CitizenId].Room, periodPrevious, 43000, 16000, "Paid", now.AddDays(-28)),
            NewInvoice(studentMap["SV003"], contractMap[studentMap["SV003"].Student.CitizenId].Room, periodCurrent, 47000, 18000, "Unpaid", now.AddDays(-2)),
            NewInvoice(studentMap["SV004"], contractMap[studentMap["SV004"].Student.CitizenId].Room, periodCurrent, 61000, 22000, "Paid", now.AddDays(-2)),
            NewInvoice(studentMap["SV005"], contractMap[studentMap["SV005"].Student.CitizenId].Room, periodCurrent, 39000, 15000, "Paid", now.AddDays(-2)),
            NewInvoice(studentMap["SV007"], contractMap[studentMap["SV007"].Student.CitizenId].Room, periodCurrent, 42000, 17000, "Paid", now.AddDays(-2)),
            NewInvoice(studentMap["SV008"], contractMap[studentMap["SV008"].Student.CitizenId].Room, periodCurrent, 59000, 21000, "Paid", now.AddDays(-2)),
            NewInvoice(studentMap["SV009"], contractMap[studentMap["SV009"].Student.CitizenId].Room, periodCurrent, 50000, 19000, "Paid", now.AddDays(-2)),
            NewInvoice(studentMap["SV010"], contractMap[studentMap["SV010"].Student.CitizenId].Room, periodCurrent, 53000, 20000, "Paid", now.AddDays(-2)),
            NewInvoice(studentMap["SV012"], contractMap[studentMap["SV012"].Student.CitizenId].Room, periodCurrent, 62000, 23000, "Unpaid", now.AddDays(-2))
        ];
    }

    private static Invoice NewInvoice(StudentSeed student, Room room, string period, decimal electricFee, decimal waterFee, string status, DateTime issuedAt)
    {
        var total = room.Price + electricFee + waterFee;
        return new Invoice
        {
            Student = student.Student,
            Room = room,
            Period = period,
            RoomFee = room.Price,
            ElectricFee = electricFee,
            WaterFee = waterFee,
            TotalAmount = total,
            Status = status,
            IssuedAt = issuedAt
        };
    }

    private static List<ElectricWaterReading> CreateElectricWaterReadings(DateTime now, IReadOnlyDictionary<string, Room> rooms)
    {
        var current = $"{now.Month:00}/{now.Year}";
        var previousDate = now.AddMonths(-1);
        var previous = $"{previousDate.Month:00}/{previousDate.Year}";
        var result = new List<ElectricWaterReading>();
        var index = 0;

        foreach (var room in rooms.Values.OrderBy(r => r.RoomCode))
        {
            var baseElectric = 100 + (index * 15);
            var baseWater = 10 + index;

            result.Add(new ElectricWaterReading
            {
                Room = room,
                Period = previous,
                OldElectric = baseElectric,
                NewElectric = baseElectric + 45,
                OldWater = baseWater,
                NewWater = baseWater + 4
            });

            result.Add(new ElectricWaterReading
            {
                Room = room,
                Period = current,
                OldElectric = baseElectric + 45,
                NewElectric = baseElectric + 95,
                OldWater = baseWater + 4,
                NewWater = baseWater + 9
            });

            index++;
        }

        return result;
    }

    private static List<ViolationRecord> CreateViolationRecords(DateTime now, IReadOnlyList<StudentSeed> students)
    {
        var map = students.ToDictionary(s => s.Code);
        return
        [
            NewViolation(map["SV001"], "Ve sinh", "Khong don dep khu vuc hanh lang theo lich truc.", now.AddDays(-18), 1),
            NewViolation(map["SV003"], "Gio giac", "Ve muon qua gio quy dinh cua KTX.", now.AddDays(-12), 1),
            NewViolation(map["SV005"], "An toan", "Su dung thiet bi dien khong dung quy dinh.", now.AddDays(-25), 2),
            NewViolation(map["SV005"], "Noi quy", "Gay on ao sau 23h.", now.AddDays(-8), 1),
            NewViolation(map["SV012"], "Ve sinh", "Khong phan loai rac dung quy dinh.", now.AddDays(-6), 1)
        ];
    }

    private static ViolationRecord NewViolation(StudentSeed student, string type, string description, DateTime date, int totalCount)
    {
        return new ViolationRecord
        {
            Student = student.Student,
            ViolationType = type,
            Description = description,
            ViolationDate = date,
            TotalCount = totalCount
        };
    }

    private static List<StudentRequest> CreateStudentRequests(DateTime now, IReadOnlyList<StudentSeed> students)
    {
        var map = students.ToDictionary(s => s.Code);
        return
        [
            new StudentRequest
            {
                Student = map["SV003"].Student,
                RequestType = "Maintenance",
                Title = "Quat tran phong A102 bi rung",
                Description = "Quat tran rung manh khi hoat dong, can kiem tra som.",
                Status = "Pending",
                CreatedAt = now.AddDays(-1)
            },
            new StudentRequest
            {
                Student = map["SV008"].Student,
                RequestType = "Other",
                Title = "Xin cap lai the noi tru",
                Description = "The noi tru bi mo chu, can ho tro cap lai.",
                Status = "Completed",
                CreatedAt = now.AddDays(-10),
                ResolvedAt = now.AddDays(-7),
                ResolutionNote = "Da cap lai the noi tru tai van phong KTX."
            },
            new StudentRequest
            {
                Student = map["SV010"].Student,
                RequestType = "Maintenance",
                Title = "Den phong D201 khong sang",
                Description = "Bong den khu ban hoc khong sang, can thay moi.",
                Status = "Approved",
                CreatedAt = now.AddDays(-5),
                ResolvedAt = now.AddDays(-4),
                ResolutionNote = "Da chuyen yeu cau cho bo phan ky thuat."
            },
            new StudentRequest
            {
                Student = map["SV001"].Student,
                RequestType = "Other",
                Title = "Dang ky su dung phong hoc chung",
                Description = "Can phong hoc chung cho nhom do an toi thu 7.",
                Status = "Rejected",
                CreatedAt = now.AddDays(-9),
                ResolvedAt = now.AddDays(-8),
                ResolutionNote = "Khung gio dang ky da du lich su dung."
            },
            new StudentRequest
            {
                Student = map["SV007"].Student,
                RequestType = "Checkout",
                Title = "Xin tra phong vao cuoi hoc ky",
                Description = "Du kien tra phong sau khi ket thuc hoc ky hien tai.",
                Status = "Pending",
                CreatedAt = now.AddDays(-2)
            }
        ];
    }

    private static List<RoomTransferRequest> CreateRoomTransferRequests(
        DateTime now,
        IReadOnlyList<StudentSeed> students,
        IReadOnlyDictionary<string, Room> rooms,
        IReadOnlyDictionary<string, SemesterPeriods> semesters)
    {
        var map = students.ToDictionary(s => s.Code);
        return
        [
            new RoomTransferRequest
            {
                Student = map["SV002"].Student,
                FromRoom = rooms["A101"],
                ToRoom = rooms["A103"],
                Reason = "Muon chuyen sang phong rong hon de thuan tien hoc tap va sinh hoat.",
                Status = "Pending",
                TransferCountInSemester = 0,
                RequestedAt = now.AddDays(-1),
                Semester = semesters["current"]
            },
            new RoomTransferRequest
            {
                Student = map["SV008"].Student,
                FromRoom = rooms["B101"],
                ToRoom = rooms["B102"],
                Reason = "Muon chuyen den phong co ban cung lop de de dang sinh hoat chung.",
                Status = "Approved",
                TransferCountInSemester = 1,
                RequestedAt = now.AddDays(-40),
                Semester = semesters["current"]
            },
            new RoomTransferRequest
            {
                Student = map["SV009"].Student,
                FromRoom = rooms["B103"],
                ToRoom = rooms["D202"],
                Reason = "Muon chuyen phong gan khu hoc tap hon nhung khong du dieu kien.",
                Status = "Rejected",
                RejectionReason = "Phong dich da duoc uu tien cho sinh vien khac.",
                TransferCountInSemester = 0,
                RequestedAt = now.AddDays(-12),
                Semester = semesters["current"]
            }
        ];
    }

    private static List<RenewalRequest> CreateRenewalRequests(
        DateTime now,
        IReadOnlyList<StudentSeed> students,
        IReadOnlyList<Contract> contracts,
        IReadOnlyDictionary<string, RenewalPackages> packages)
    {
        var studentMap = students.ToDictionary(s => s.Code, s => s.Student);
        var contractMap = contracts.ToDictionary(c => c.Student.CitizenId);

        return
        [
            new RenewalRequest
            {
                Student = studentMap["SV007"],
                Contract = contractMap[studentMap["SV007"].CitizenId],
                RenewalPackage = packages["1ky"],
                Status = "Pending",
                RequestedAt = now.AddDays(-2)
            },
            new RenewalRequest
            {
                Student = studentMap["SV001"],
                Contract = contractMap[studentMap["SV001"].CitizenId],
                RenewalPackage = packages["2ky"],
                Status = "Approved",
                RequestedAt = now.AddDays(-20)
            },
            new RenewalRequest
            {
                Student = studentMap["SV005"],
                Contract = contractMap[studentMap["SV005"].CitizenId],
                RenewalPackage = packages["1nam"],
                Status = "Rejected",
                RequestedAt = now.AddDays(-7),
                RejectionReason = "Sinh vien co qua nhieu vi pham noi quy trong hoc ky."
            }
        ];
    }

    private static List<Notification> CreateNotifications(DateTime now, IReadOnlyList<User> admins, IReadOnlyList<StudentSeed> students)
    {
        return
        [
            NewNotification(admins[0], "Co don dang ky moi", "Sinh vien Bui Van Long vua nop don dang ky phong A103.", false, now.AddHours(-12)),
            NewNotification(admins[1], "Co yeu cau chuyen phong", "Sinh vien Tran Quoc Binh dang cho duyet chuyen tu A101 sang A103.", false, now.AddHours(-8)),
            NewNotification(admins[2], "Co yeu cau gia han", "Sinh vien Nguyen Thi Lan vua gui yeu cau gia han hop dong.", false, now.AddHours(-6)),
            NewNotification(students.First(s => s.Code == "SV003").User, "Hoa don thang moi", "Hoa don ky hien tai cua ban da duoc tao va dang o trang thai chua thanh toan.", false, now.AddDays(-1)),
            NewNotification(students.First(s => s.Code == "SV008").User, "Yeu cau da hoan thanh", "Yeu cau cap lai the noi tru cua ban da duoc xu ly xong.", true, now.AddDays(-6))
        ];
    }

    private static Notification NewNotification(User user, string title, string message, bool isRead, DateTime createdAt)
    {
        return new Notification
        {
            User = user,
            Title = title,
            Message = message,
            IsRead = isRead,
            CreatedAt = createdAt
        };
    }

    private static List<Facility> CreateFacilities(DateTime now, IReadOnlyDictionary<string, Room> rooms)
    {
        var result = new List<Facility>();

        foreach (var room in rooms.Values)
        {
            result.Add(new Facility
            {
                Room = room,
                Name = "Giuong",
                Quantity = room.Capacity,
                Status = "Good",
                CreatedAt = now.AddMonths(-4)
            });

            result.Add(new Facility
            {
                Room = room,
                Name = "Ban hoc",
                Quantity = room.Capacity,
                Status = "Good",
                CreatedAt = now.AddMonths(-4)
            });

            result.Add(new Facility
            {
                Room = room,
                Name = "Quat tran",
                Quantity = room.Capacity >= 8 ? 2 : 1,
                Status = room.RoomCode is "A102" or "D201" ? "Damaged" : "Good",
                CreatedAt = now.AddMonths(-4)
            });

            result.Add(new Facility
            {
                Room = room,
                Name = "Tu do",
                Quantity = Math.Max(2, room.Capacity / 2),
                Status = room.RoomCode == "C202" ? "UnderMaintenance" : "Good",
                CreatedAt = now.AddMonths(-4)
            });
        }

        return result;
    }

    private static async Task BackfillRoomPricesAsync(AppDbContext context)
    {
        var rooms = context.Rooms.Where(r => r.Price <= 0).ToList();
        if (rooms.Count == 0) return;

        foreach (var room in rooms)
        {
            room.Price = GetDefaultRoomPrice(room.Capacity);
        }

        var contracts = context.Contracts
            .Where(c => c.Price <= 0)
            .Join(
                context.Rooms,
                contract => contract.RoomId,
                room => room.Id,
                (contract, room) => new { Contract = contract, room.Price })
            .ToList();

        foreach (var item in contracts)
        {
            item.Contract.Price = item.Price;
        }

        await context.SaveChangesAsync();
    }

    private static decimal GetDefaultRoomPrice(int capacity)
    {
        return capacity switch
        {
            4 => 450000,
            6 => 350000,
            8 => 300000,
            _ => 250000
        };
    }

    private sealed record StudentSeed(string Code, User User, Student Student, Relative Relative);
}
