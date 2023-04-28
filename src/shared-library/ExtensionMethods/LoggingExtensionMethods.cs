using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Exceptions;
using Serilog.Formatting;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;
using Serilog.Templates;
using Serilog.Templates.Themes;
using shared_library.Services.ConfigServiceCommonFolder;
using shared_library.Services.LoggingServicesFolder;
using Throw;

namespace shared_library.ExtensionMethods;

public static class LoggingExtensionMethods
{
    // Used so you can switch logging frameworks faster.
    // Instead of changing all "LogContext.PushProperty" in your code, you just need to change this one Method to fit your logging framework.
    public static IDisposable PushProperty(this Microsoft.Extensions.Logging.ILogger logger, string name, object value)
    {
        return LogContext.PushProperty(name, value);
    }

    public static IHostBuilder UseCustomLoggingService(this IHostBuilder builder, ExpressionTemplate? expressionTemplate = null)
    {
        builder.ConfigureServices(collection =>
        {
            collection.AddSingleton<IConfigServiceCommon, ConfigServiceCommon>();
            collection.AddSingleton<LoggingLevelSwitchService>();
            collection.AddHostedService<LoggingLevelSwitchCheckerBackgroundTask>();
        });

        builder.UseSerilog((_, provider, loggerConfiguration) =>
        {
            var configServiceCommon = provider.GetService<IConfigServiceCommon>();
            var loggingLevelSwitchService = provider.GetService<LoggingLevelSwitchService>();

            configServiceCommon.ThrowIfNull();
            loggingLevelSwitchService.ThrowIfNull();

            expressionTemplate ??= ExpressionTemplate;
            loggerConfiguration.CreateCustomLoggerConfiguration(configServiceCommon,
                loggingLevelSwitchService.LoggingLevelSwitch, expressionTemplate);
        });

        return builder;
    }

    private static void CreateCustomLoggerConfiguration(this LoggerConfiguration loggerConfiguration,
        IConfigServiceCommon configServiceCommon, LoggingLevelSwitch loggingLevelSwitch,
        ITextFormatter expressionTemplate)
    {
        loggerConfiguration.MinimumLevel.ControlledBy(loggingLevelSwitch)
            .Enrich.WithProperty("ApplicationName", configServiceCommon.APPLICATION_NAME)
            .Enrich.FromLogContext()
            .Enrich.WithCorrelationId()
            .Enrich.WithExceptionDetails()
            .WriteTo.Console(expressionTemplate);

        if (!string.IsNullOrWhiteSpace(configServiceCommon.SEQ_URL))
        {
            loggerConfiguration.WriteTo.Seq(configServiceCommon.SEQ_URL, apiKey: configServiceCommon.SEQ_API_KEY);
        }

        if (!string.IsNullOrWhiteSpace(configServiceCommon.GRAYLOG_URL) && configServiceCommon.GRAYLOG_PORT is not null)
        {
            loggerConfiguration.WriteTo.Graylog(configServiceCommon.GRAYLOG_URL, configServiceCommon.GRAYLOG_PORT.Value,
                configServiceCommon.GRAYLOG_PROTOCOL?.ToLower() is "tcp" ? TransportType.Tcp : TransportType.Udp);
        }
    }

    private static readonly ExpressionTemplate ExpressionTemplate =
        new(
            "[{@t:yyyy-MM-dd HH:mm:ss.fff zzz} | {@l:u3}]" +
            "{#if SourceContext is not null} [{SourceContext:l}]{#end}" +
            "{#if CorrelationId is not null} [CorrelationId: {CorrelationId}]{#end}" +
            "{#if TraceId is not null} [TraceId: {TraceId}]{#end}" +
            "{#if ServerId is not null} [ServerId: {ServerId}]{#end}" +
            " {@m}\n{@x}",
            theme: TemplateTheme.Code);
}