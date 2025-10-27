using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace Server.Auth;

public class ApiKeyMiddleware : IMiddleware
{
    private readonly ClientsConfig _cfg;
    
    // ovo polje da ogranicim replay napade(kad vec radim hmac)
    private static readonly TimeSpan AllowedSkew = TimeSpan.FromMinutes(2); // dozvoljeno odstupanje

    public ApiKeyMiddleware(ClientsConfig cfg)
    {
        _cfg = cfg;
    }

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        // osnovni headeri koji moraju postojati
        if (!ctx.Request.Headers.TryGetValue("X-Client-Id", out var cidVals))
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await ctx.Response.WriteAsync("Missing X-Client-Id");
            return;
        }

        var clientId = cidVals.ToString();

        if (!_cfg.TryGet(clientId, out var entry) || entry is null)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await ctx.Response.WriteAsync("Unknown client");
            return;
        }

        // api kljuc kojim odredjijem koji je to klijent
        if (!ctx.Request.Headers.TryGetValue("X-API-Key", out var keyVals) ||
            !string.Equals(entry.apiKey, keyVals.ToString(), StringComparison.Ordinal))
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await ctx.Response.WriteAsync("Invalid API key");
            return;
        }

        // provera hmac potpisa(X-Ts i X-Sign)
        if (!ctx.Request.Headers.TryGetValue("X-Ts", out var tsVal) ||
            !ctx.Request.Headers.TryGetValue("X-Sign", out var sigVal))
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await ctx.Response.WriteAsync("Missing signature headers");
            return;
        }

        // timestamp validacija
        if (!long.TryParse(tsVal, out var ts))
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await ctx.Response.WriteAsync("Invalid timestamp format");
            return;
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - ts) > AllowedSkew.TotalSeconds)
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await ctx.Response.WriteAsync("Stale/invalid timestamp");
            return;
        }

        // hash tela zahteva
        string bodyHashHex = "";
        if (ctx.Request.ContentLength > 0)
        {
            ctx.Request.EnableBuffering();
            using var sha = SHA256.Create();
            using var ms = new MemoryStream();
            await ctx.Request.Body.CopyToAsync(ms);
            var bodyBytes = ms.ToArray();
            var hash = sha.ComputeHash(bodyBytes);
            bodyHashHex = Convert.ToHexString(hash);
            ctx.Request.Body.Position = 0;
        }

        // rekonstrukcija potpisnog stringa
        var method = ctx.Request.Method.ToUpperInvariant();
        var pathAndQuery = ctx.Request.Path + ctx.Request.QueryString;
        var signingString = $"{method}\n{pathAndQuery}\n{tsVal}\n{bodyHashHex}";

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(entry.apiKey));
        var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(signingString)));

        // sig je u heks formatu
        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(sigVal.ToString())))
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await ctx.Response.WriteAsync("Bad signature");
            return;
        }

        // uspesna autentifikacija
        ctx.Items["ClientId"] = clientId;
        ctx.Items["Role"] = entry.role;
        await next(ctx);
    }
}
