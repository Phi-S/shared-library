using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace shared_library_example_console_application;

public class TestBackgroundTask : IHostedService
{
    private readonly ILogger<TestBackgroundTask> _logger;

    public TestBackgroundTask(ILogger<TestBackgroundTask> logger)
    {
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("blabla started");

        var _ = Task.Run((async () =>
        {
            await Task.Delay(1000, cancellationToken);
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(400, cancellationToken);

                _logger.LogTrace("Trace...");
                _logger.LogDebug("Debug...");
                _logger.LogInformation("Info...");
                _logger.LogWarning("Warn..." );
                _logger.LogError("Error...");
                _logger.LogCritical("Crit..." );
                Log.Fatal("Fatal...");
            }
        }), cancellationToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}