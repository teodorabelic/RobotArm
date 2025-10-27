using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

public static class HmacSigner
{
    public static async Task SignAsync(HttpRequestMessage req, string apiKey)
    {
        var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        var method = req.Method.Method.ToUpperInvariant();
        var path = req.RequestUri!.PathAndQuery;

        string bodyHashHex = "";
        if (req.Content != null)
        {
            var originalContent = req.Content;
            var bodyBytes = await originalContent.ReadAsByteArrayAsync();

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(bodyBytes);
            bodyHashHex = Convert.ToHexString(hash);

            var newContent = new ByteArrayContent(bodyBytes);
            foreach (var header in originalContent.Headers)
                newContent.Headers.TryAddWithoutValidation(header.Key, header.Value);

            req.Content = newContent;
        }

        var signingString = $"{method}\n{path}\n{ts}\n{bodyHashHex}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(apiKey));
        var sig = hmac.ComputeHash(Encoding.UTF8.GetBytes(signingString));
        var sigHex = Convert.ToHexString(sig);

        req.Headers.Remove("X-Ts");
        req.Headers.Remove("X-Sign");
        req.Headers.Add("X-Ts", ts);
        req.Headers.Add("X-Sign", sigHex);
    }
}
