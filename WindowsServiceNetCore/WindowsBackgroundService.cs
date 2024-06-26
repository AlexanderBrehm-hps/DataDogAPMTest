using System.Diagnostics;

namespace WindowsServiceNetCore
{
    public sealed class WindowsBackgroundService : BackgroundService
    {
        MyService _myService;
        ILogger<WindowsBackgroundService> _logger;

        public WindowsBackgroundService(MyService myService, ILogger<WindowsBackgroundService> logger)
        {
            _myService = myService;
            _logger = logger;

            var message = $"In OnStart{Environment.NewLine}";
            message += $"Launched from {Environment.CurrentDirectory}{Environment.NewLine}";
            message += $"Physical location {AppDomain.CurrentDomain.BaseDirectory}{Environment.NewLine}";
            message += $"AppContext.BaseDir {AppContext.BaseDirectory}{Environment.NewLine}";
            message += $"Runtime Call {Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)}";
            _logger.LogInformation(message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation($"{_myService.MakeHttpRequest(false)}");
                    _logger.LogInformation($"{_myService.MakeWebRequest(false)}");
                    _logger.LogInformation($"{_myService.MakeProcessRequest(false)}");
                    string output = _myService.DoIntensiveWork();
                    _logger.LogInformation($"{output}", output);

                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // When the stopping token is canceled, for example, a call made from services.msc,
                // we shouldn't exit with a non-zero exit code. In other words, this is expected...
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);

                // Terminates this process and returns an exit code to the operating system.
                // This is required to avoid the 'BackgroundServiceExceptionBehavior', which
                // performs one of two scenarios:
                // 1. When set to "Ignore": will do nothing at all, errors cause zombie services.
                // 2. When set to "StopHost": will cleanly stop the host, and log errors.
                //
                // In order for the Windows Service Management system to leverage configured
                // recovery options, we need to terminate the process with a non-zero exit code.
                Environment.Exit(1);
            }
        }
    }
}
