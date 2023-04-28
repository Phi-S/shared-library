using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using shared_library.Services.ConfigServiceCommonFolder;

namespace shared_library.Services.LoggingServicesFolder;

public class LoggingLevelSwitchCheckerBackgroundTask : IHostedService, IDisposable
{
    private readonly ILogger<LoggingLevelSwitchCheckerBackgroundTask> _logger;
    private readonly IConfigServiceCommon _configServiceCommon;
    private readonly LoggingLevelSwitchService _loggingLevelSwitchService;
    private Timer? _timer;

    public LoggingLevelSwitchCheckerBackgroundTask(ILogger<LoggingLevelSwitchCheckerBackgroundTask> logger,
        IConfigServiceCommon configServiceCommon, LoggingLevelSwitchService loggingLevelSwitchService)
    {
        _logger = logger;
        _configServiceCommon = configServiceCommon;
        _loggingLevelSwitchService = loggingLevelSwitchService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{TaskName} started", nameof(LoggingLevelSwitchCheckerBackgroundTask));
        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        return Task.CompletedTask;
    }

    private void DoWork(object? state)
    {
        var logLevelAsString = _configServiceCommon.MINIMUM_LOG_LEVEL;
        var currentMinimumLogLevel = _loggingLevelSwitchService.LoggingLevelSwitch.MinimumLevel;
        var newMinimumLogLevel = GetLogLevelFromString(logLevelAsString);

        if (currentMinimumLogLevel == newMinimumLogLevel || newMinimumLogLevel == null) return;
        _loggingLevelSwitchService.LoggingLevelSwitch.MinimumLevel = newMinimumLogLevel.Value;
        _logger.LogInformation("Log level has changed from \"{CurrentMinimumLogLevel}\" to \"{NewMinimumLogLevel}\"",
            currentMinimumLogLevel.ToString(), newMinimumLogLevel.ToString());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("{TaskName} stopped", nameof(LoggingLevelSwitchCheckerBackgroundTask));
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }

    private static LogEventLevel? GetLogLevelFromString(string logLevelAsString)
    {
        if (StringEqualsEnumValueIgnoreCase(logLevelAsString, LogEventLevel.Verbose))
        {
            return LogEventLevel.Verbose;
        }

        if (StringEqualsEnumValueIgnoreCase(logLevelAsString, LogEventLevel.Debug))
        {
            return LogEventLevel.Debug;
        }

        if (StringEqualsEnumValueIgnoreCase(logLevelAsString, LogEventLevel.Information))
        {
            return LogEventLevel.Information;
        }

        if (StringEqualsEnumValueIgnoreCase(logLevelAsString, LogEventLevel.Warning))
        {
            return LogEventLevel.Warning;
        }

        if (StringEqualsEnumValueIgnoreCase(logLevelAsString, LogEventLevel.Error))
        {
            return LogEventLevel.Error;
        }

        if (StringEqualsEnumValueIgnoreCase(logLevelAsString, LogEventLevel.Fatal))
        {
            return LogEventLevel.Fatal;
        }

        return null;
    }

    private static bool StringEqualsEnumValueIgnoreCase(string stringToCompare, Enum enumToCompare)
    {
        return stringToCompare.Equals(enumToCompare.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}