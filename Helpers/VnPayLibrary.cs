using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace BackendAPI.Helpers;

public class VnPayLibrary
{
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData[key] = value;
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData[key] = value;
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }

    // ================= CREATE PAYMENT URL =================
    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        var data = new StringBuilder();

        foreach (var (key, value) in _requestData)
        {
            data.Append(Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value) + "&");
        }

        var queryString = data.ToString();
        if (queryString.Length > 0)
        {
            queryString = queryString.Remove(queryString.Length - 1, 1);
        }

        var signData = queryString;

        var secureHash = HmacSha512(vnpHashSecret, signData);

        return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
    }

    // ================= VALIDATE SIGNATURE =================
    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var rspRaw = GetResponseRaw();
        var myChecksum = HmacSha512(secretKey, rspRaw);

        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string GetResponseRaw()
    {
        var data = new StringBuilder();

        var sorted = new SortedList<string, string>(_responseData, new VnPayCompare());

        // KHÔNG mutate dữ liệu gốc
        sorted.Remove("vnp_SecureHashType");
        sorted.Remove("vnp_SecureHash");

        foreach (var (key, value) in sorted)
        {
            data.Append(Uri.EscapeDataString(key) + "=" + Uri.EscapeDataString(value) + "&");
        }

        var result = data.ToString();
        if (result.Length > 0)
        {
            result = result.Remove(result.Length - 1, 1);
        }

        return result;
    }

    // ================= HASH =================
    private string HmacSha512(string key, string inputData)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);

        using var hmac = new HMACSHA512(keyBytes);
        var hashValue = hmac.ComputeHash(inputBytes);

        return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
    }

    // ================= GET IP =================
    public static string GetIpAddress(HttpContext context)
    {
        try
        {
            var ip = context.Connection.RemoteIpAddress;

            if (ip != null)
            {
                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    ip = Dns.GetHostEntry(ip)
                        .AddressList
                        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                }

                return ip?.ToString() ?? "127.0.0.1";
            }
        }
        catch { }

        return "127.0.0.1";
    }

    // ================= SORT =================
    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string? x, string? y)
        {
            return string.CompareOrdinal(x, y);
        }
    }
}