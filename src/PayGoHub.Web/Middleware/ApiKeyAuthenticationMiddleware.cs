using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using PayGoHub.Infrastructure.Data;

namespace PayGoHub.Web.Middleware;

/// <summary>
/// Middleware for API key authentication - validates API-KEY or X-API-Key headers
/// </summary>
public class ApiKeyAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeaderName = "API-KEY";
    private const string XApiKeyHeaderName = "X-API-Key";

    public ApiKeyAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, PayGoHubDbContext dbContext)
    {
        // Skip authentication for non-API routes
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        // Skip authentication for health check
        if (context.Request.Path.StartsWithSegments("/api/health"))
        {
            await _next(context);
            return;
        }

        // Try to get API key from headers (support both API-KEY and X-API-Key)
        string? apiKey = null;

        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValue))
        {
            apiKey = apiKeyHeaderValue.FirstOrDefault();
        }
        else if (context.Request.Headers.TryGetValue(XApiKeyHeaderName, out var xApiKeyHeaderValue))
        {
            apiKey = xApiKeyHeaderValue.FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { status = "error", code = "missing_api_key", message = "API key is required" });
            return;
        }

        // Hash the API key for comparison
        var apiKeyHash = ComputeSha256Hash(apiKey);

        // Look up the API client
        var apiClient = await dbContext.ApiClients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.ApiKeyHash == apiKeyHash && c.IsActive);

        if (apiClient == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { status = "error", code = "invalid_api_key", message = "Invalid API key" });
            return;
        }

        // Validate IP whitelist if configured
        if (!string.IsNullOrEmpty(apiClient.IpWhitelist))
        {
            var clientIp = context.Connection.RemoteIpAddress?.ToString();
            var allowedIps = apiClient.IpWhitelist.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (clientIp != null && !allowedIps.Contains(clientIp))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { status = "error", code = "ip_not_allowed", message = "IP address not allowed" });
                return;
            }
        }

        // Store API client info in HttpContext for use in controllers
        context.Items["ApiClientId"] = apiClient.Id;
        context.Items["ApiClientName"] = apiClient.Name;
        context.Items["ApiClientScopes"] = apiClient.AllowedScopes;
        context.Items["ApiClientProviders"] = apiClient.AllowedProviders;

        // Update last used timestamp asynchronously (fire and forget)
        _ = UpdateLastUsedAsync(context.RequestServices, apiClient.Id);

        await _next(context);
    }

    private static string ComputeSha256Hash(string rawData)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawData));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static async Task UpdateLastUsedAsync(IServiceProvider services, Guid apiClientId)
    {
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PayGoHubDbContext>();
            await db.ApiClients
                .Where(c => c.Id == apiClientId)
                .ExecuteUpdateAsync(c => c.SetProperty(x => x.LastUsedAt, DateTime.UtcNow));
        }
        catch
        {
            // Silently ignore update failures - this is non-critical
        }
    }
}

/// <summary>
/// Extension methods for adding API key authentication middleware
/// </summary>
public static class ApiKeyAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseApiKeyAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ApiKeyAuthenticationMiddleware>();
    }
}
