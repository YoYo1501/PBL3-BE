using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace BackendAPI.Configurations;

public static class SwaggerConfigurationExtensions
{
    public static IServiceCollection AddSwaggerConfigurationSetup(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Backend API", Version = "v1" });
            c.SchemaFilter<SwaggerDefaultValues>(); // Đăng ký Swashbuckle schema filter
            
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Chỉ dán token vào đây (KHÔNG cần 'Bearer')"
            });
            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }
}

public class SwaggerDefaultValues : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null)
            return;

        foreach (var property in schema.Properties)
        {
            if ((property.Value.Type == "integer" || property.Value.Type == "number") && property.Value.Default == null)
            {
                if (property.Key.ToLower().Contains("month") || property.Key.ToLower().Contains("duration"))
                    property.Value.Example = new OpenApiInteger(6);
                else if (property.Key.ToLower().Contains("year") || property.Key.ToLower().Contains("nam"))
                    property.Value.Example = new OpenApiInteger(2026);
                else if (property.Key.ToLower().Contains("amount") || property.Key.ToLower().Contains("fee") || property.Key.ToLower().Contains("price"))
                    property.Value.Example = new OpenApiDouble(1500000.0);
                else if (property.Key.ToLower().Contains("quantity") || property.Key.ToLower().Contains("soluong"))
                    property.Value.Example = new OpenApiInteger(1);
                else if (property.Key.ToLower().Contains("id"))
                    property.Value.Example = new OpenApiInteger(1);
                else
                    property.Value.Example = new OpenApiInteger(0);
            }
            else if (property.Value.Type == "string" && property.Value.Format == "date-time" && property.Value.Default == null)
            {
                property.Value.Example = new OpenApiString(DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ"));
            }
            else if (property.Value.Type == "string" && property.Value.Default == null)
            {
                if (property.Key.ToLower().Contains("email"))
                    property.Value.Example = new OpenApiString("admin@gmail.com");
                else if (property.Key.ToLower().Contains("password") || property.Key.ToLower().Contains("mk"))
                    property.Value.Example = new OpenApiString("Admin@123");
                else if (property.Key.ToLower().Contains("phone"))
                    property.Value.Example = new OpenApiString("0987654321");
                else if (property.Key.ToLower().Contains("citizenid") || property.Key.ToLower().Contains("cccd"))
                    property.Value.Example = new OpenApiString("048200012345");
                else if (property.Key.ToLower().Contains("name"))
                    property.Value.Example = new OpenApiString("Nguyen Van A");
                else if (property.Key.ToLower().Contains("gender"))
                    property.Value.Example = new OpenApiString("Male");
                else if (property.Key.ToLower().Contains("roomcode") || property.Key.ToLower().Contains("room"))
                    property.Value.Example = new OpenApiString("A101");
                else if (property.Key.ToLower().Contains("period"))
                    property.Value.Example = new OpenApiString("10/2023");
                else if (property.Key.ToLower().Contains("address") || property.Key.ToLower().Contains("diachi"))
                    property.Value.Example = new OpenApiString("123 Duong Le Duan, Da Nang");
                else if (property.Key.ToLower().Contains("department") || property.Key.ToLower().Contains("khoa"))
                    property.Value.Example = new OpenApiString("CNTT");
                else if (property.Key.ToLower().Contains("class") || property.Key.ToLower().Contains("lop"))
                    property.Value.Example = new OpenApiString("20T2");
                else if (property.Key.ToLower().Contains("studentcode") || property.Key.ToLower().Contains("masv"))
                    property.Value.Example = new OpenApiString("102200111");
                else if (property.Key.ToLower().Contains("reason") || property.Key.ToLower().Contains("lydo") || property.Key.ToLower().Contains("description") || property.Key.ToLower().Contains("mota"))
                    property.Value.Example = new OpenApiString("Noi dung chi tiet o day...");
                else if (property.Key.ToLower().Contains("title") || property.Key.ToLower().Contains("tieude"))
                    property.Value.Example = new OpenApiString("Tieu de mau");
                else if (property.Key.ToLower().Contains("type") || property.Key.ToLower().Contains("loai"))
                    property.Value.Example = new OpenApiString("Other");
                else if (property.Key.ToLower().Contains("status") || property.Key.ToLower().Contains("trangthai"))
                    property.Value.Example = new OpenApiString("Pending");
                else if (property.Key.ToLower().Contains("relativename"))
                    property.Value.Example = new OpenApiString("Nguyen Van B");
                else if (property.Key.ToLower().Contains("relativephone"))
                    property.Value.Example = new OpenApiString("0987654322");
                else if (property.Key.ToLower().Contains("relationship"))
                    property.Value.Example = new OpenApiString("Father");
                else if (property.Key.ToLower().Contains("message") || property.Key.ToLower().Contains("thongdiep"))
                    property.Value.Example = new OpenApiString("Test message");
                else if (property.Key.ToLower().Contains("url"))
                    property.Value.Example = new OpenApiString("https://example.com");
                else if (property.Key.ToLower().Contains("code") || property.Key.ToLower().Contains("ma"))
                    property.Value.Example = new OpenApiString("CODE123");
                else
                    property.Value.Example = new OpenApiString("string");
            }
        }
    }
}

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SchemaFilter<SwaggerDefaultValues>();

            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Backend API",
                Version = "v1"
            });

            // ✅ FIX JWT HERE
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Chỉ dán chuỗi Token của bạn vào đây (KHÔNG cần nhập chữ 'Bearer').\r\n\r\nVí dụ: eyJhbGci..."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
        });

        return services;
    }
}