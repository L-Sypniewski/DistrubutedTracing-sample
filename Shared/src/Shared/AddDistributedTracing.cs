using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Shared;

public static class TracingExtensions
{
    public static IServiceCollection AddDistributedTracing(this IServiceCollection services, string serviceName)
    {

        services.AddOpenTelemetryMetrics(builder => // TODO: to be used needs to be configured in otel-collector-config.yaml
        {
            builder.AddAspNetCoreInstrumentation();
            builder.AddHttpClientInstrumentation();
            builder.AddMeter($"{serviceName}_AddMeter");
            builder.AddOtlpExporter(options => options.Protocol = OtlpExportProtocol.Grpc);
        });


        services.AddLogging(x =>
        {
            x.AddOpenTelemetry(builder =>
            {
                builder.IncludeFormattedMessage = false;
                builder.IncludeScopes = false;
                builder.ParseStateValues = false;
                builder
                .AddConsoleExporter()
                .AddOtlpExporter(options =>
                {
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
            });
        });
        services.AddOpenTelemetryTracing(builder =>
            {
                builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                // EFCore instrumentation appears inside .AddService("webapisecondservice") whereas SqlClientInstrumentation appears at the same level. There's no need to use both EFCore and SqlClientInstrumentation if only EFCore is used
                .AddService("webapisecondservice", serviceNamespace: "webapisecondservice_namespace", serviceVersion: "webapisecondservice_version1"));

                builder.AddAspNetCoreInstrumentation(x =>
                {
                    x.EnableGrpcAspNetCoreSupport = true;
                    x.RecordException = true;
                    x.Enrich = (activity, eventName, rawObject) =>
                               {
                                   activity.SetTag($"AspNetCore.webapisecond.customtag.eventname", eventName);
                                   activity.SetTag($"AspNetCore.webapisecond.customtag.rawObjectType", rawObject.GetType().ToString());
                               };
                });

                builder.AddHttpClientInstrumentation(c =>
                            {
                                c.SetHttpFlavor = true;
                                c.RecordException = true;
                                c.Enrich = (activity, eventName, rawObject) =>
                               {
                                   activity.SetTag($"httpclient.webapisecond.customtag.eventname", eventName);
                                   activity.SetTag($"httpclient.webapisecond.customtag.rawObjectType", rawObject.GetType().ToString());

                               };
                            });

                builder.AddEntityFrameworkCoreInstrumentation(options =>
                {
                    options.SetDbStatementForText = true;
                    options.SetDbStatementForStoredProcedure = true;
                });
                // builder.AddSqlClientInstrumentation(options =>
                // {
                //     options.EnableConnectionLevelAttributes = true;
                //     options.RecordException = true;
                //     options.SetDbStatementForText = true;
                //     options.SetDbStatementForStoredProcedure = true;
                //     options.Enrich = (activity, eventName, rawObject) =>
                //     {
                //         if (!eventName.Equals("OnCustom"))
                //         {
                //             return;
                //         }
                //     };
                // })

                builder.AddOtlpExporter(options =>
                {
                    options.Protocol = OtlpExportProtocol.Grpc;
                });
                // .AddConsoleExporter();
            });

        return services;
    }
}