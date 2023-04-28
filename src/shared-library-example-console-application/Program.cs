using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using shared_library.ExtensionMethods;

namespace shared_library_example_console_application;

public abstract class Program
{
    public static async Task Main()
    {
        try
        {
            var builder = Host.CreateDefaultBuilder();
            builder.UseCustomLoggingService();
            builder.ConfigureServices(collection =>
            {
                collection.AddHostedService<TestBackgroundTask>();
            });

            var host = builder.Build();
            await host.RunAsync();
        }
        finally
        {
            Log.Information("Application closed");
            await Log.CloseAndFlushAsync();
        }
    }
}