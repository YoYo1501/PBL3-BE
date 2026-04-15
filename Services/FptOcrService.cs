using System.Text.Json;
using System.Text.Json.Serialization;
using BackendAPI.Models.DTOs.Ocr;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BackendAPI.Services;

public class FptOcrService : IOcrService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public FptOcrService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["FptAi:ApiKey"] ?? string.Empty;
    }

    public async Task<CccdInformationDto> ExtractCccdInfoAsync(IFormFile image)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new Exception("API Key c?a FPT.AI ch?a ???c c?u hình trong appsettings.");
        }

        // FPT.AI API Endpoint: https://api.fpt.ai/vision/idr/vnm
        var requestUrl = "https://api.fpt.ai/vision/idr/vnm";

        using var content = new MultipartFormDataContent();
        
        using var stream = image.OpenReadStream();
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(image.ContentType);
        
        content.Add(streamContent, "image", image.FileName);

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("api-key", _apiKey);

        var response = await _httpClient.PostAsync(requestUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorDetail = await response.Content.ReadAsStringAsync();
            throw new Exception($"Không th? g?i API FPT.AI, Status: {response.StatusCode}. Details: {errorDetail}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var fptResult = JsonSerializer.Deserialize<FptOcrResponse>(jsonResponse, options);

        if (fptResult == null || fptResult.ErrorCode != 0 || fptResult.Data == null || fptResult.Data.Count == 0)
        {
            throw new Exception($"FPT.AI tr? v? l?i ho?c không nh?n di?n ???c: {fptResult?.ErrorMessage}");
        }

        var detectedData = fptResult.Data[0];

        return new CccdInformationDto
        {
            IdNumber = detectedData.Id,
            FullName = detectedData.Name,
            DateOfBirth = detectedData.Dob,
            Gender = detectedData.Sex,
            Address = detectedData.Address,
            HomeTown = detectedData.Home,
            Nationality = detectedData.Nationality
        };
    }

    // Các l?p n?i b? ?? mapping JSON tr? v? t? FPT
    private class FptOcrResponse
    {
        [JsonPropertyName("errorCode")]
        public int ErrorCode { get; set; }

        [JsonPropertyName("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonPropertyName("data")]
        public List<FptDataField> Data { get; set; }
    }

    private class FptDataField
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("dob")] public string Dob { get; set; }
        [JsonPropertyName("sex")] public string Sex { get; set; }
        [JsonPropertyName("address")] public string Address { get; set; }
        [JsonPropertyName("home")] public string Home { get; set; }
        [JsonPropertyName("nationality")] public string Nationality { get; set; }
    }
}