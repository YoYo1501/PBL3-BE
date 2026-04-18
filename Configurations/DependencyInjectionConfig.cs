using Microsoft.Extensions.DependencyInjection;
using BackendAPI.Repositories;
using BackendAPI.Repositories.Interfaces;
using BackendAPI.Services;
using BackendAPI.Services.Interfaces;

namespace BackendAPI.Configurations
{
    public static class DependencyInjectionConfig
    {
        public static IServiceCollection AddProjectDependencies(this IServiceCollection services)
        {
            // Repositories
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddScoped<IRegistrationRepository, RegistrationRepository>();
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IProfileRepository, ProfileRepository>();
            services.AddScoped<IRoomTransferRepository, RoomTransferRepository>();
            services.AddScoped<IViolationRepository, ViolationRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IRevenueRepository, RevenueRepository>();
            services.AddScoped<IStudentRepository, StudentRepository>();
            services.AddScoped<INotificationRepository, NotificationRepository>();
            services.AddScoped<IStudentRequestRepository, StudentRequestRepository>();
            services.AddScoped<IFacilityRepository, FacilityRepository>();
            services.AddScoped<IContractRepository, ContractRepository>();

            // Services
            services.AddHttpClient();
            services.AddScoped<IOcrService, TesseractOcrService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRegistrationService, RegistrationService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<IViolationService, ViolationService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IRevenueService, RevenueService>();
            services.AddScoped<IStudentService, StudentService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IStudentRequestService, StudentRequestService>();
            services.AddScoped<IFacilityService, FacilityService>();
            services.AddScoped<IRoomTransferService, RoomTransferService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IPaymentService, VnPayService>();
            services.AddTransient<IEmailService, EmailService>();

            return services;
        }
    }
}
