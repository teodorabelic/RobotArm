using System.Net;

namespace Server.Auth;

public class ApiKeyMiddleware : IMiddleware
{
    private readonly ClientsConfig _cfg;
    public ApiKeyMiddleware(ClientsConfig cfg) { _cfg = cfg; }

    public async Task InvokeAsync(HttpContext ctx, RequestDelegate next)
    {
        if (!ctx.Request.Headers.TryGetValue("X-API-Key", out var keyVals) ||
            !ctx.Request.Headers.TryGetValue("X-Client-Id", out var cidVals))
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await ctx.Response.WriteAsync("Missing headers");
            return;
        }

        var key = keyVals.ToString();
        var clientId = cidVals.ToString();

        if (!_cfg.TryGet(clientId, out var entry) ||
            entry is null ||
            !string.Equals(entry.apiKey, key, StringComparison.Ordinal))
        {
            ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await ctx.Response.WriteAsync("Invalid API key/client");
            return;
        }

        ctx.Items["ClientId"] = clientId;
        ctx.Items["Role"] = entry.role; // entry nije null do ove tacke
        await next(ctx);
    }
}
