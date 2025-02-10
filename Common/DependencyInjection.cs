using MachineTrading.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace MachineTrading.Common;

public static class DependencyInjection
{
    public static void AddSerilog(this WebApplicationBuilder builder)
    {
        var log = new LoggerConfiguration()
            .WriteTo.MSSqlServer(
                connectionString: "Server=.;Database=MachineTrading;Trusted_Connection=True;TrustServerCertificate=True",
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "Logs", 
                    AutoCreateSqlTable = true
                },
                restrictedToMinimumLevel: LogEventLevel.Warning
            ).CreateLogger();
        
        builder.Services.AddSerilog(log);
    }

    public static void AddDependencies(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<PlaywrightService>();
    }
}