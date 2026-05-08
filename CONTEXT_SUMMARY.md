# Context Summary

Cap nhat: 2026-05-08

Muc dich cua file nay: dung lam bo nho ngan gon cho lan lam viec tiep theo voi AI/Codex. Khi can tiet kiem token, hay doc file nay truoc, roi chi mo them cac file lien quan den task cu the.

## Tom Tat Hoi Thoai

- User yeu cau tao file markdown de luu tom tat hoi thoai, cac y chinh va kien truc he thong.
- Muc tieu la lan sau chi can nap file nay thay vi doc lai toan bo hoi thoai.
- Da bo sung chuc nang bien lai cho backend va cap nhat giao dien sinh vien muc `Hoa don, bien lai`.

## Cap Nhat 2026-05-08 - Hoa Don Va Bien Lai

- Nghiep vu moi:
  - Tab `Hoa don` chi hien cac hoa don chua thanh toan (`Status == "Unpaid"`).
  - Khi thanh toan thanh cong, hoa don duoc danh dau `Paid` va chuyen sang tab `Bien lai`.
  - Tab `Bien lai` lay du lieu tu endpoint rieng, khong con loc hoa don da thanh toan o frontend neu endpoint bien lai tra duoc du lieu.
- Backend da them endpoint bien lai:
  - Student: `GET /api/receipts/my`
  - Student: `GET /api/receipts/my/{invoiceId}`
  - Student: `GET /api/receipts/my/{invoiceId}/download`
  - Admin: `GET /api/receipts`
  - Admin: `GET /api/receipts/{invoiceId}`
  - Admin: `GET /api/receipts/{invoiceId}/download`
- Backend da them metadata thanh toan vao `Invoice`:
  - `PaidAt`
  - `PaymentMethod`
  - `TransactionCode`
- Migration moi:
  - `Migrations/20260508012600_AddInvoicePaymentMetadata.cs`
- Frontend da cap nhat:
  - `FrontendWeb/js/student.js`: tach `currentInvoices` va `currentReceipts`, goi dong thoi `/invoices/my` va `/receipts/my`.
  - `FrontendWeb/css/student.css`: tinh chinh man hinh hoa don/bien lai theo mockup: card thong ke 4 cot, tab co icon, alert thong tin, bang hoa don, nut thanh toan/tai hoa don xep doc, panel chi tiet ben phai.
- Sau thanh toan VNPAY thanh cong:
  - Frontend mo lai `section-invoice`.
  - Neu `paymentStatus=success`, tu dong chuyen sang tab `Bien lai`.
  - Toast hien thong bao hoa don da duoc chuyen sang bien lai.
- Kiem tra da chay:
  - Backend: `dotnet build -c Release`
  - Frontend JS: `node --check js/student.js`

## Project Hien Tai

- Ten project: `BackendAPI`
- Loai project: he thong quan ly ky tuc xa gom Backend API + Frontend static web.
- Solution/project chinh: `BackendAPI.sln`, `BackendAPI.csproj`
- Target framework: `.NET 10.0` (`net10.0`)
- C# language version: `14.0`
- Nullable va implicit usings dang bat.
- Backend path dang lam viec: `D:\1.University\semester-4\semester-4\3.PBL3\BackendAPI\BackendAPI`
- Frontend path that su dung: `D:\1.University\semester-4\semester-4\3.PBL3\BackendAPI\FrontendWeb`
- Co mot thu muc `D:\...\3.PBL3\FrontendWeb` o cap khac nhung luc kiem tra khong thay noi dung; uu tien FE trong `BackendAPI\FrontendWeb`.

## Tech Stack

- ASP.NET Core Web API
- Entity Framework Core + SQL Server
- JWT Bearer authentication
- Swagger/Swashbuckle
- BCrypt.Net-Next cho password hashing
- EPPlus cho import/export Excel
- Tesseract + ImageSharp + ZXing cho OCR/anh/quet ma
- VnPay payment integration
- Email service
- Frontend static HTML/CSS/vanilla JavaScript, khong thay `package.json` hay bundler.

## Cach Chay Nhanh

Backend:

- Cau hinh launch:
  - HTTP: `http://localhost:5280`
  - HTTPS: `https://localhost:7077`
- Lenh thuong dung:
  - `dotnet run`
  - `dotnet run -- --seed-only` de seed data roi thoat
- Connection string nam trong `appsettings.json` hoac `appsettings.Development.json`, key `ConnectionStrings:DefaultConnection`.
- Khong nen copy gia tri secret/token/password vao file context nay.

Frontend:

- FE la static web, co the mo truc tiep cac file trong `FrontendWeb/pages/`.
- Trang dang nhap chinh: `FrontendWeb/pages/login.html`.
- Trang sinh vien: `FrontendWeb/pages/student.html`.
- Trang admin: `FrontendWeb/pages/admin.html`.
- Trang dang ky: `FrontendWeb/pages/register.html`.
- API base dang hardcode ve backend HTTP local:
  - `FrontendWeb/js/api.js`: `http://localhost:5280/api`
  - `FrontendWeb/pages/login.html`: `http://localhost:5280`
  - `FrontendWeb/pages/register.html`: `http://localhost:5280/api`
- Neu chay qua static server thi can backend dang chay o `http://localhost:5280`; backend hien dang CORS `AllowAll`.

## Kien Truc Tong Quan

Luon di theo luong chinh:

`Controllers -> Services/Interfaces -> Repositories/Interfaces -> AppDbContext -> Entities`

Luon map voi FE theo luong:

`pages/*.html -> js/auth.js + js/api.js -> js/student.js hoac js/admin/*.js -> Backend API`

Thu muc chinh:

- `Controllers/`: API endpoints.
- `Services/`: xu ly nghiep vu.
- `Services/Interfaces/`: contract cua service layer.
- `Repositories/`: truy van/cap nhat DB.
- `Repositories/Interfaces/`: contract cua repository layer.
- `Data/AppDbContext.cs`: EF Core DbContext, DbSet, relationship, soft delete, unique index, decimal precision.
- `Data/SeedData.cs`: seed du lieu khi app start.
- `Models/Entities/`: entity DB.
- `Models/DTOs/`: request/response DTOs theo domain.
- `Configurations/`: Swagger, JWT auth, dependency injection.
- `Middleware/ErrorHandlerMiddleware.cs`: bat exception va tra JSON.
- `Converters/CustomDateTimeConverter.cs`: converter DateTime cho JSON.

Frontend:

- `FrontendWeb/pages/`: cac man hinh HTML.
- `FrontendWeb/js/api.js`: API wrapper chung.
- `FrontendWeb/js/auth.js`: token/role guard/logout/confirm modal.
- `FrontendWeb/js/student.js`: toan bo logic trang sinh vien.
- `FrontendWeb/js/admin/`: logic trang admin tach theo domain/module.
- `FrontendWeb/css/`: style cho login/admin/student.
- `FrontendWeb/assets/images/`: logo va background.
- `FrontendWeb/index.html`: gan nhu placeholder rong, khong phai entry chinh.

## Runtime Setup Trong `Program.cs`

- Clear logging providers, them Console va Debug logging.
- Add controllers va them `CustomDateTimeConverter`.
- Add Swagger.
- Add `AppDbContext` voi SQL Server tu `DefaultConnection`.
- Add JWT authentication.
- Add CORS policy `AllowAll`.
- Dang ky dependency injection bang `AddProjectDependencies`.
- Add memory cache.
- Dung `ErrorHandlerMiddleware`.
- Development moi bat Swagger UI.
- Dung HTTPS redirection, CORS, authentication, authorization.
- Map controllers.
- Goi `SeedData.SeedAsync(context)` khi app start.
- Neu co arg `--seed-only` thi seed xong thoat.

## Dependency Injection Chinh

Repositories duoc dang ky scoped:

- Room, Registration, Auth, Profile, RoomTransfer, Violation
- Invoice, Revenue, Student, Notification, StudentRequest
- Facility, Contract

Services duoc dang ky scoped/transient:

- `IOcrService -> TesseractOcrService`
- `IPaymentService -> VnPayService`
- `IEmailService -> EmailService` la transient
- Cac service domain con lai dang ky scoped theo ten tuong ung.
- Co `AddHttpClient()`.

## Domain/Nghiep Vu Chinh

- Auth: login bang citizen id/password, tra JWT.
- Profile: xem/sua ho so, doi mat khau.
- Rooms: danh sach phong, phong con trong, phong cua sinh vien, CRUD admin.
- Registrations: dang ky o ky tuc xa, OCR CCCD, admin duyet ho so.
- Contracts: hop dong hien tai cua sinh vien, gia han hop dong, admin duyet gia han, CRUD contract.
- Room transfers: sinh vien xem phong co the chuyen, hold phong, gui yeu cau, admin duyet, sinh vien huy.
- Invoices: import chi so dien nuoc Excel, generate draft, publish invoice, xem/tra thu cong/export/nhac no.
- Payments: tao URL thanh toan VnPay va xu ly return URL.
- Revenue: thong ke va export doanh thu cho admin.
- Students: quan ly danh sach sinh vien cho admin.
- Notifications: admin tao/sua/xoa/xem, user xem thong bao cua minh va mark as read.
- Student requests: sinh vien gui yeu cau checkout/maintenance/other, admin cap nhat trang thai.
- Facilities: quan ly trang thiet bi trong phong.
- Violations: tra cuu va them vi pham cho sinh vien.

## Frontend Tong Quan

FE khong dung framework. Tat ca function nam global scope, nen thu tu `<script>` trong HTML rat quan trong.

Auth/session:

- `login.html` goi `POST /api/auth/login`.
- Khi login thanh cong, luu `token`, `role`, `fullName`, `mustChangePassword`.
- Neu tick "Ghi nho dang nhap" thi luu vao `localStorage`; neu khong thi luu vao `sessionStorage`.
- `auth.js` co:
  - `getToken()`
  - `getRole()`
  - `requireRole(role)`
  - `logout()`
  - `showAppConfirm(options)`
- `api.js` tu dong gan `Authorization: Bearer <token>` cho API can auth.
- Neu API tra `401`, `api.js` xoa token/role va redirect ve `login.html`.

API helpers:

- `callApi(endpoint, options)`: request co token, JSON mac dinh.
- `callApiPublic(endpoint, options)`: request public.
- `callApiUpload(endpoint, formData)`: upload multipart/form-data, co token neu co.
- `callApiBlob(endpoint, options)`: tai file/blob co token.
- Helpers hien thi: `formatDate`, `formatCurrency`, `statusBadge`.

Trang public:

- `pages/login.html`: UI dang nhap + quen mat khau dang xu ly inline trong file.
- `pages/register.html`: form dang ky nhieu buoc, load phong trong qua `GET /room/available`, OCR CCCD qua `POST /registrations/extract-cccd`, gui dang ky qua `POST /registrations`.

Trang sinh vien:

- `pages/student.html` load `auth.js`, `api.js`, goi `requireRole('Student')`, roi load `js/student.js`.
- `student.js` xu ly:
  - profile, cap nhat lien he, doi mat khau
  - thong tin phong va hop dong
  - gia han hop dong
  - hoa don va thanh toan VnPay
  - yeu cau sinh vien
  - yeu cau chuyen phong
  - co so vat chat va bao hong
  - thong bao va mark read
- Endpoint sinh vien dang dung nhieu:
  - `/profile`, `/profile/change-password`
  - `/room/my-room`
  - `/contracts/my`, `/contracts/renewal-packages`, `/contracts/renew`
  - `/invoices/my`
  - `/payments/create-payment-url/{invoiceId}`
  - `/studentrequests`, `/studentrequests/my`, `/studentrequests/{id}/cancel`
  - `/roomtransfers/available`, `/roomtransfers/hold`, `/roomtransfers`, `/roomtransfers/my`, `/roomtransfers/{id}/cancel`
  - `/notifications/my`, `/notifications/{id}/read`
  - `/facilities/room/{roomId}`

Trang admin:

- `pages/admin.html` load `auth.js`, `api.js`, goi `requireRole('Admin')`, sau do load cac module trong `js/admin/`.
- Thu tu module quan trong: `admin-core.js`, `admin-state.js`, `admin-pagination.js`, `admin-layout.js`, cac module domain, cuoi cung `admin-main.js`.
- `admin-main.js` bind UI va load data ban dau:
  - header/navigation/reload/pagination
  - registrations, requests, transfers, renewals
  - contracts, invoices, rooms, facilities, students, revenue, notifications
- `admin-state.js` giu bien global nhu selected ids, danh sach items, page state.
- `admin-pagination.js` quan ly paging cho registrations/requests/transfers/renewals/invoices/rooms/facilities/students/revenue/notifications.
- Cac module admin:
  - `admin-overview.js`: dashboard tong quan pending + phong.
  - `admin-profile.js`: profile admin va doi mat khau.
  - `admin-registrations.js`: duyet/tuchoi dang ky.
  - `admin-requests.js`: xu ly yeu cau sinh vien.
  - `admin-transfers.js`: duyet/tuchoi chuyen phong.
  - `admin-renewals.js`: duyet/tuchoi gia han hop dong.
  - `admin-contracts.js`: list/detail/update/delete contract.
  - `admin-invoices.js`: import chi so, generate, publish, remind debt, export, pay manual.
  - `admin-rooms.js`: CRUD phong.
  - `admin-facilities.js`: CRUD thiet bi.
  - `admin-students.js`: list/detail/update/delete sinh vien.
  - `admin-revenue.js`: thong ke/export doanh thu.
  - `admin-notifications.js`: gui/sua/xoa thong bao, inbox admin, mark read.

## Frontend File Can Mo Theo Task

- Login/auth/token: `pages/login.html`, `js/auth.js`, `js/api.js`.
- Dang ky/OCR/phong trong: `pages/register.html`.
- Student dashboard: `pages/student.html`, `js/student.js`, `css/student.css`.
- Admin dashboard/layout: `pages/admin.html`, `js/admin/admin-main.js`, `js/admin/admin-layout.js`, `js/admin/admin-state.js`, `css/admin.css`.
- Admin module nao thi mo file `js/admin/admin-<domain>.js` tuong ung.
- Doi API base: sua `js/api.js`, dong API trong `login.html`, va API trong `register.html`.

## Controllers Va Quyen Truy Cap

Route mac dinh la `api/[controller]`.

- `AuthController`: `POST api/Auth/login`.
- `RoomController`: public xem phong; Student xem `my-room`; Admin tao/sua/xoa.
- `RegistrationsController`: public OCR/dang ky; Admin xem pending va approve.
- `ContractsController`: Student xem/gia han; Admin xem pending renewals, approve, CRUD.
- `InvoicesController`: Admin import/generate/draft/publish/list/detail/pay/remind/export; Student xem `my`.
- `PaymentsController`: Student tao payment URL; VnPay return la GET.
- `RevenueController`: Admin only.
- `StudentsController`: Admin only.
- `NotificationsController`: can auth; Admin quan ly; user xem `my`, mark read.
- `StudentRequestsController`: can auth; user tao/xem/huy cua minh; Admin list/update status.
- `RoomTransfersController`: can auth; Student available/my/hold/submit/cancel; Admin pending/approve.
- `FacilitiesController`: public xem; Admin tao/sua/xoa.
- `ProfileController`: authenticated user.

## Data Model Chinh

Entities chinh:

- `User`: account, role `Student`/`Admin`, password hash, active state, must change password.
- `Student`: thong tin sinh vien, lien ket 1-1 voi `User`.
- `Relative`: nguoi than cua sinh vien.
- `Building`: toa nha, gender allowed, danh sach phong.
- `Room`: phong, capacity, occupancy, status, price, soft delete.
- `Registration`: ho so dang ky phong, status pending/approved/rejected.
- `Contract`: hop dong phong cua sinh vien, start/end/status/price.
- `RenewalPackages`: goi gia han.
- `RenewalRequest`: yeu cau gia han hop dong.
- `SemesterPeriods`: hoc ky, registration open flag.
- `RoomTransferRequest`: yeu cau chuyen phong.
- `Invoice`: hoa don phong/dien/nuoc, status unpaid/paid.
- `ElectricWaterReading`: chi so dien nuoc theo period.
- `ViolationRecord`: lich su vi pham.
- `Notification`: thong bao user, soft delete.
- `StudentRequest`: yeu cau checkout/maintenance/other.
- `Facility`: thiet bi trong phong, soft delete.

Soft delete:

- `Student`, `Room`, `Facility`, `Notification` implement `ISoftDelete`.
- `AppDbContext.SaveChanges/SaveChangesAsync` chuyen delete thanh set `IsDeleted = true`.
- Query filter an cac record `IsDeleted`.

Unique indexes dang co:

- `User.CitizenId`
- `Student.CitizenId`
- `Room.RoomCode`
- `Contract.ContractCode`
- `Registration.RegistrationCode`

## Quy Uoc Code

- Service thuong tra tuple dang `(bool Success, string Message, Data?)`.
- Repository va service dung async/await.
- Paging dung `PagedResultDto<T>` trong `Models/DTOs/Common/PagingDtos.cs`.
- DTO chia theo domain trong `Models/DTOs/<Domain>/Requests|Responses`.
- Status hien dang dung string, vi du `Pending`, `Approved`, `Rejected`, `Active`, `Unpaid`.
- Controller lay `studentId`/`userId` tu JWT claims o nhieu endpoint co auth.
- Loi nghiep vu co the nem `BadRequestException`; middleware tra JSON.

## Diem Can Chu Y

- `JwtAuthConfig` dang `ValidateLifetime = false`; neu len production nen bat lai validate token lifetime.
- `Program.cs` va `JwtAuthConfig` dang bat log PII/security artifact de debug; khong nen giu khi production.
- CORS dang `AllowAnyOrigin/AllowAnyMethod/AllowAnyHeader`.
- Co fallback JWT key trong code; nen dung secret tu configuration/environment khi production.
- Mot so comment tieng Viet trong source bi loi encoding; khi sua file nen can than de khong lam hong them encoding.
- Thu muc `bin/`, `obj/`, `artifacts_check/` va cac log `test-api-*.log` la output/artifact, thuong khong can doc tru khi debug runtime.
- FE cung co nhieu text tieng Viet; khi sua nen giu UTF-8 va kiem tra lai hien thi tren trinh duyet.
- FE dang hardcode URL local. Neu deploy hoac doi port backend, phai sua cac diem API base da liet ke.
- `callApi` mac dinh set `Content-Type: application/json`; khi upload/file phai dung `callApiUpload` hoac helper phu hop.
- Vi FE la vanilla JS global, tranh doi ten function/bien global trung nhau giua cac file admin.

## Trang Thai Git Luc Tao File

- Khi tao file nay, `git status --short` dang bao `?? BackendAPI.sln`.
- File moi duoc them: `CONTEXT_SUMMARY.md`.
- FE la mot git repo rieng trong `FrontendWeb`.
- Khi bo sung thong tin FE, repo FE dang co san thay doi chua commit:
  - `css/student.css`
  - `js/student.js`
  - `pages/register.html`
  - `pages/student.html`
- Nhung thay doi FE tren khong phai do lan tao context nay tao ra.

## Huong Dan Cho Lan Sau

1. Doc file `CONTEXT_SUMMARY.md` truoc.
2. Neu task cham vao domain nao, mo controller/service/repository/DTO/entity cua domain do.
3. Neu task lien quan DB, doc `Data/AppDbContext.cs` va migrations gan nhat.
4. Neu task lien quan auth/JWT, doc `Configurations/JwtAuthConfig.cs`, `AuthService.cs`, `AuthRepository.cs`, `AuthController.cs`.
5. Neu task lien quan API behavior, doc controller va service truoc khi sua.
6. Neu task lien quan FE, doc `FrontendWeb/js/api.js`, `FrontendWeb/js/auth.js`, HTML page tuong ung, va JS module tuong ung.
7. Sau khi sua backend, chay build/test phu hop, toi thieu `dotnet build` neu co the.
8. Sau khi sua frontend, mo trang HTML tuong ung tren browser va test luong co lien quan voi backend dang chay.

## Quy Uoc Tra Loi User

- Sau khi lam xong task nho, chi bao ngan gon `Xong.`.
- Khong can lap lai chi tiet kieu da sua file nao, dong nao, noi dung da sua gi, tru khi user hoi.
- Chi noi them khi co loi, blocker, can user quyet dinh, hoac co canh bao quan trong.
