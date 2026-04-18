using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BackendAPI.Models.DTOs.Ocr;
using BackendAPI.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Tesseract;
using ZXing;
using ZXing.Common;

namespace BackendAPI.Services;

public class TesseractOcrService(IWebHostEnvironment environment) : IOcrService
{
    private static readonly Regex MultiSpaceRegex = new(@"\s+", RegexOptions.Compiled);
    private static readonly Regex CitizenIdRegex = new(@"\b\d{12}\b", RegexOptions.Compiled);
    private static readonly Regex DateRegex = new(@"\b\d{2}/\d{2}/\d{4}\b", RegexOptions.Compiled);

    public async Task<CccdInformationDto> ExtractCccdInfoAsync(IFormFile image)
    {
        if (image == null || image.Length == 0)
        {
            throw new InvalidOperationException("Vui lòng tải lên ảnh CCCD.");
        }

        var tessdataPath = ResolveTessdataPath();
        EnsureLanguageDataExists(tessdataPath);

        await using var stream = new MemoryStream();
        await image.CopyToAsync(stream);
        var sourceBytes = stream.ToArray();

        var qrDto = TryExtractFromQr(sourceBytes);
        if (!string.IsNullOrWhiteSpace(qrDto.IdNumber) && !string.IsNullOrWhiteSpace(qrDto.FullName))
        {
            return qrDto;
        }

        using var engine = new TesseractEngine(tessdataPath, "vie+eng", EngineMode.Default);
        engine.DefaultPageSegMode = PageSegMode.Auto;
        engine.SetVariable("preserve_interword_spaces", "1");
        engine.SetVariable("user_defined_dpi", "300");

        var attempts = BuildImageVariants(sourceBytes);
        OcrCandidate? best = null;

        foreach (var attempt in attempts)
        {
            using var pix = Pix.LoadFromMemory(attempt.ImageBytes);
            using var page = engine.Process(pix);
            var rawText = page.GetText() ?? string.Empty;
            var lines = rawText
                .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(CleanLine)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            var dto = MapToDto(lines);
            var candidate = new OcrCandidate(attempt.Label, lines, dto, Score(dto, lines));

            if (best == null || candidate.Score > best.Score)
            {
                best = candidate;
            }
        }

        if (best == null || (string.IsNullOrWhiteSpace(best.Dto.IdNumber) && string.IsNullOrWhiteSpace(best.Dto.FullName)))
        {
            throw new InvalidOperationException(
                "Không đọc được thông tin CCCD từ ảnh. Hãy chụp thẳng mặt thẻ, đủ sáng và để thẻ chiếm phần lớn khung hình.");
        }

        return best.Dto;
    }

    private static CccdInformationDto TryExtractFromQr(byte[] sourceBytes)
    {
        using var original = Image.Load<Rgba32>(sourceBytes);
        var rotations = new[] { 0, 90, 270, 180 };

        foreach (var rotation in rotations)
        {
            using var image = original.Clone(ctx =>
            {
                ctx.AutoOrient();
                if (rotation != 0)
                {
                    ctx.Rotate(rotation);
                }
            });

            var payload = DecodeQrPayload(image);
            if (string.IsNullOrWhiteSpace(payload))
            {
                continue;
            }

            var dto = ParseQrPayload(payload);
            if (!string.IsNullOrWhiteSpace(dto.IdNumber))
            {
                return dto;
            }
        }

        return new CccdInformationDto();
    }

    private static IEnumerable<ImageVariant> BuildImageVariants(byte[] sourceBytes)
    {
        using var original = Image.Load<Rgba32>(sourceBytes);
        var rotations = new[] { 0, 90, 270, 180 };

        foreach (var rotation in rotations)
        {
            yield return new ImageVariant($"rotate-{rotation}", RenderVariant(original, rotation, enhance: false));
            yield return new ImageVariant($"rotate-{rotation}-enhanced", RenderVariant(original, rotation, enhance: true));
        }
    }

    private static byte[] RenderVariant(Image<Rgba32> source, float rotation, bool enhance)
    {
        using var clone = source.Clone(ctx =>
        {
            ctx.AutoOrient();

            if (rotation != 0)
            {
                ctx.Rotate(rotation);
            }

            // Tăng kích thước nhẹ để Tesseract đọc phần chữ nhỏ trên CCCD tốt hơn.
            var width = Math.Max(source.Width, 1600);
            ctx.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(width, 0),
                Sampler = KnownResamplers.Lanczos3
            });

            if (enhance)
            {
                ctx.Grayscale();
                ctx.Contrast(1.25f);
                ctx.GaussianSharpen(0.8f);
            }
        });

        using var output = new MemoryStream();
        clone.Save(output, new PngEncoder());
        return output.ToArray();
    }

    private static string DecodeQrPayload(Image<Rgba32> image)
    {
        var raw = new byte[image.Width * image.Height * 4];
        image.CopyPixelDataTo(raw);

        var luminance = new RGBLuminanceSource(raw, image.Width, image.Height, RGBLuminanceSource.BitmapFormat.RGBA32);
        var reader = new BarcodeReaderGeneric
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE },
                TryHarder = true,
                TryInverted = true
            }
        };

        var result = reader.Decode(luminance);
        return result?.Text ?? string.Empty;
    }

    private static CccdInformationDto ParseQrPayload(string payload)
    {
        var dto = new CccdInformationDto();
        var parts = payload
            .Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(CleanLine)
            .ToList();

        if (!parts.Any())
        {
            return dto;
        }

        dto.IdNumber = parts.FirstOrDefault(part => Regex.IsMatch(part, @"^\d{12}$")) ?? string.Empty;
        dto.DateOfBirth = parts.FirstOrDefault(part => DateRegex.IsMatch(part)) ?? string.Empty;
        dto.Gender = parts.FirstOrDefault(part => MatchesAny(part, "Nam", "Nữ", "Nu")) switch
        {
            var gender when string.Equals(gender, "Nam", StringComparison.OrdinalIgnoreCase) => "Nam",
            var gender when !string.IsNullOrWhiteSpace(gender) => "Nữ",
            _ => string.Empty
        };
        dto.Nationality = parts.FirstOrDefault(part => MatchesAny(part, "Việt Nam", "Viet Nam")) ?? "Việt Nam";

        var nameIndex = !string.IsNullOrWhiteSpace(dto.IdNumber) ? parts.IndexOf(dto.IdNumber) + 1 : -1;
        if (nameIndex > 0 && nameIndex < parts.Count)
        {
            dto.FullName = parts[nameIndex];
        }
        else
        {
            dto.FullName = parts.FirstOrDefault(part => part.Any(char.IsLetter) && !part.Any(char.IsDigit) && !MatchesAny(part, "Nam", "Nữ", "Nu", "Việt Nam", "Viet Nam")) ?? string.Empty;
        }

        dto.Address = parts.LastOrDefault(part =>
            part.Contains(',') ||
            MatchesAny(part, "Phường", "Phuong", "Quận", "Quan", "Tỉnh", "Tinh", "Thừa Thiên", "Thua Thien", "Huế", "Hue")) ?? string.Empty;

        return dto;
    }

    private CccdInformationDto MapToDto(IReadOnlyList<string> lines)
    {
        return new CccdInformationDto
        {
            IdNumber = ExtractCitizenId(lines),
            FullName = ExtractField(lines, ["HỌ VÀ TÊN", "HO VA TEN", "FULL NAME"], acceptNumericContent: false),
            DateOfBirth = ExtractDateOfBirth(lines),
            Gender = ExtractGender(lines),
            Address = ExtractField(lines, ["NƠI THƯỜNG TRÚ", "NOI THUONG TRU", "ADDRESS"]),
            HomeTown = ExtractField(lines, ["QUÊ QUÁN", "QUE QUAN", "PLACE OF ORIGIN"]),
            Nationality = ExtractField(lines, ["QUỐC TỊCH", "QUOC TICH", "NATIONALITY"], fallbackValue: "Việt Nam")
        };
    }

    private static int Score(CccdInformationDto dto, IReadOnlyList<string> lines)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(dto.IdNumber))
        {
            score += 60;
        }

        if (!string.IsNullOrWhiteSpace(dto.FullName))
        {
            score += 20;
        }

        if (!string.IsNullOrWhiteSpace(dto.DateOfBirth))
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(dto.Address))
        {
            score += 5;
        }

        if (lines.Any(line => ContainsAnyKeyword(line, ["CĂN CƯỚC CÔNG DÂN", "CAN CUOC CONG DAN"])))
        {
            score += 10;
        }

        if (lines.Any(line => ContainsAnyKeyword(line, ["HỌ VÀ TÊN", "HO VA TEN"])))
        {
            score += 10;
        }

        return score;
    }

    private string ResolveTessdataPath()
    {
        var candidates = new[]
        {
            Path.Combine(environment.ContentRootPath, "tessdata"),
            Path.Combine(Directory.GetParent(environment.ContentRootPath)?.FullName ?? environment.ContentRootPath, "tessdata"),
            Path.Combine(AppContext.BaseDirectory, "tessdata"),
            @"C:\Program Files\Tesseract-OCR\tessdata",
            @"C:\Program Files (x86)\Tesseract-OCR\tessdata"
        };

        return candidates.FirstOrDefault(Directory.Exists)
            ?? Path.Combine(environment.ContentRootPath, "tessdata");
    }

    private static void EnsureLanguageDataExists(string tessdataPath)
    {
        var vieFile = Path.Combine(tessdataPath, "vie.traineddata");
        var engFile = Path.Combine(tessdataPath, "eng.traineddata");

        if (File.Exists(vieFile) && File.Exists(engFile))
        {
            return;
        }

        throw new InvalidOperationException(
            $"Thiếu dữ liệu OCR. Hãy đặt 'vie.traineddata' và 'eng.traineddata' vào thư mục '{tessdataPath}'.");
    }

    private static string ExtractCitizenId(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            var match = CitizenIdRegex.Match(line);
            if (match.Success)
            {
                return match.Value;
            }
        }

        return string.Empty;
    }

    private static string ExtractDateOfBirth(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (!ContainsAnyKeyword(line, ["NGÀY SINH", "NGAY SINH", "DATE OF BIRTH"]))
            {
                continue;
            }

            var match = DateRegex.Match(line);
            if (match.Success)
            {
                return match.Value;
            }
        }

        return lines
            .Select(line => DateRegex.Match(line))
            .FirstOrDefault(match => match.Success)
            ?.Value ?? string.Empty;
    }

    private static string ExtractGender(IEnumerable<string> lines)
    {
        foreach (var line in lines)
        {
            if (!ContainsAnyKeyword(line, ["GIỚI TÍNH", "GIOI TINH", "SEX"]))
            {
                continue;
            }

            var normalized = NormalizeForMatching(line);
            if (normalized.Contains("NU"))
            {
                return "Nữ";
            }

            if (normalized.Contains("NAM"))
            {
                return "Nam";
            }
        }

        return string.Empty;
    }

    private static string ExtractField(
        IReadOnlyList<string> lines,
        IEnumerable<string> keywords,
        string fallbackValue = "",
        bool acceptNumericContent = true)
    {
        for (var index = 0; index < lines.Count; index++)
        {
            var line = lines[index];
            if (!ContainsAnyKeyword(line, keywords))
            {
                continue;
            }

            var sameLineValue = ExtractValueAfterKeyword(line);
            if (IsUsefulValue(sameLineValue, acceptNumericContent))
            {
                return sameLineValue;
            }

            if (index + 1 < lines.Count && IsUsefulValue(lines[index + 1], acceptNumericContent))
            {
                return lines[index + 1];
            }
        }

        return fallbackValue;
    }

    private static bool IsUsefulValue(string value, bool acceptNumericContent)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!acceptNumericContent && value.Any(char.IsDigit))
        {
            return false;
        }

        return true;
    }

    private static bool ContainsAnyKeyword(string line, IEnumerable<string> keywords)
    {
        var normalized = NormalizeForMatching(line);
        return keywords.Any(keyword => normalized.Contains(NormalizeForMatching(keyword)));
    }

    private static bool MatchesAny(string text, params string[] values)
    {
        var normalized = NormalizeForMatching(text);
        return values.Any(value => normalized.Contains(NormalizeForMatching(value)));
    }

    private static string ExtractValueAfterKeyword(string line)
    {
        var parts = line.Split(':', 2, StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]))
        {
            return parts[1];
        }

        var normalized = NormalizeForMatching(line);
        if (normalized.StartsWith("HO VA TEN") || normalized.StartsWith("HỌ VÀ TÊN"))
        {
            return string.Join(' ', line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(3));
        }

        return string.Empty;
    }

    private static string CleanLine(string line)
    {
        return MultiSpaceRegex.Replace(line.Replace('|', ' ').Trim(), " ");
    }

    private static string NormalizeForMatching(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToUpperInvariant(ch));
            }
        }

        return builder.ToString()
            .Replace('Đ', 'D')
            .Replace(":", string.Empty)
            .Trim();
    }

    private sealed record ImageVariant(string Label, byte[] ImageBytes);
    private sealed record OcrCandidate(string Label, IReadOnlyList<string> Lines, CccdInformationDto Dto, int Score);
}
