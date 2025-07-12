using Serilog;
using Serilog.Events;

namespace StudentTracker.Api.Configurations;

public static class SerilogConfig
{
    public static IHostBuilder ConfigureSerilog(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, services, loggerConfiguration) =>
        {
            loggerConfiguration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithEnvironmentName()
                .Enrich.WithThreadId()
                .WriteTo.Console();

            if (context.HostingEnvironment.IsProduction())
            {
                loggerConfiguration.MinimumLevel.Override("Microsoft", LogEventLevel.Warning);
            }
        });
    }
}
