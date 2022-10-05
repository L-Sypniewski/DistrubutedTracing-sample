using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Shared;

public record Traces(string ParentActivityId, string IdFormat, string Id, string TraceId,
 string SpanId, string Parent, string ParentSpanId,
  string RootId, string TraceState, string TraceTags);

public class TraceProviderMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TraceProviderMiddleware> _logger;
    private readonly bool _printToConsole;
    public TraceProviderMiddleware(RequestDelegate next, ILogger<TraceProviderMiddleware>? logger, bool printToConsole)
    {
        _next = next;
        _logger = logger ?? NullLogger<TraceProviderMiddleware>.Instance;
        _printToConsole = printToConsole;
    }

    public Task Invoke(HttpContext httpContext)
    {
        var traces = CreateTraces();
        if (_printToConsole)
        {
            Console.WriteLine($"Traces: {traces}");
        }
        else
        {
            _logger.LogTrace("Traces: {Traces}", traces);
        }
        return _next(httpContext);
    }

    private static Traces? CreateTraces()
    {
        if (Activity.Current is null)
        {
            return null;
        }

        Activity currentActivity = Activity.Current;
        var IdFormat = currentActivity.IdFormat.ToString();
        var ParentActivityId = currentActivity.Parent?.Id ?? "";
        var Id = currentActivity.Id ?? "";
        var TraceId = currentActivity.TraceId.ToString();
        var SpanId = currentActivity.SpanId.ToString();
        var ParentId = currentActivity.ParentId ?? "";
        var ParentSpanId = currentActivity.ParentSpanId.ToString();
        var RootId = currentActivity.RootId ?? "";
        var TraceState = currentActivity.TraceStateString ?? "";
        var TraceTags = string.Join(" | ", currentActivity.Tags.Select(pair => pair.Value));


        return new Traces(ParentActivityId, IdFormat, Id, TraceId, SpanId, ParentId, ParentSpanId, RootId, TraceState, TraceTags);
    }
}

// Extension method used to add the middleware to the HTTP request pipeline.
public static class TraceProviderMiddlewareExtensions
{
    public static IApplicationBuilder UseTraceProvider(this IApplicationBuilder builder, bool printToConsole = false)
    {
        return builder.UseMiddleware<TraceProviderMiddleware>(printToConsole);
    }
}


