using BackendAPI.Models.DTOs.Ocr;
using Microsoft.AspNetCore.Http;

namespace BackendAPI.Services.Interfaces;

public interface IOcrService
{
    Task<CccdInformationDto> ExtractCccdInfoAsync(IFormFile image);
}