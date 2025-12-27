using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UserManagement.Middleware;

public sealed class RequestResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

    public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log request
        context.Request.EnableBuffering();

        string requestBody = string.Empty;
        if (context.Request.ContentLength > 0)
        {
            context.Request.Body.Position = 0;
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
            requestBody = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        _logger.LogInformation("Incoming request {Method} {Path} Headers:{Headers} Body:{Body}",
            context.Request.Method,
            context.Request.Path,
            JsonSerializer.Serialize(context.Request.Headers, new JsonSerializerOptions { WriteIndented = false }),
            string.IsNullOrEmpty(requestBody) ? "<empty>" : (requestBody.Length > 2000 ? requestBody[..2000] + "…(truncated)" : requestBody)
        );

        // Capture response
        var originalBodyStream = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);

            context.Response.Body.Seek(0, SeekOrigin.Begin);
            string responseText = await new StreamReader(context.Response.Body).ReadToEndAsync();
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            _logger.LogInformation("Outgoing response {StatusCode} Headers:{Headers} Body:{Body}",
                context.Response.StatusCode,
                JsonSerializer.Serialize(context.Response.Headers, new JsonSerializerOptions { WriteIndented = false }),
                string.IsNullOrEmpty(responseText) ? "<empty>" : (responseText.Length > 2000 ? responseText[..2000] + "…(truncated)" : responseText)
            );

            // Copy back to original stream
            await responseBody.CopyToAsync(originalBodyStream);
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }
}