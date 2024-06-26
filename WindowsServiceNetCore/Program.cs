using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;
using System.Diagnostics;

namespace WindowsServiceNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Services.AddWindowsService(options =>
            {
                options.ServiceName = "Hps WindowsServicenetCore";
            });

            LoggerProviderOptions.RegisterProviderOptions<
                EventLogSettings,
                EventLogLoggerProvider>(builder.Services);

            builder.Services.AddSingleton<MyService>();
            builder.Services.AddHostedService<WindowsBackgroundService>();

            var host = builder.Build();
            host.Run();
        }
    }
}