using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.IO;
using System.Net.Http;

namespace WindowsServiceNetFW
{
    public partial class WindowsServiceNetFW : ServiceBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private System.Timers.Timer timer;
        private int eventId = 1;

        public WindowsServiceNetFW()
        {
            InitializeComponent();

            string eventSource = "Hps-WindowsServiceNetFW";
            string logName = "Application";

            if (!EventLog.SourceExists(eventSource))
                EventLog.CreateEventSource(eventSource, logName);

            eventLog1.Source = eventSource;
            eventLog1.Log = logName;

            // Set up a timer that triggers every minute.
            timer = new System.Timers.Timer()
            {
                Interval = 60000 // 1 minute
                //Interval = 30000 // 30 seconds
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
        }

        public string MakeProcessRequest(bool shouldRunLikeCrap)
        {
            var stopWatch = new Stopwatch();
            var process = new Process
            {
                StartInfo = new ProcessStartInfo("cmd.exe", "/c timeout 1")
            };

            stopWatch.Start();
            process.Start();
            process.WaitForExit();
            stopWatch.Stop();
            return $"Shelled out for {stopWatch.ElapsedMilliseconds} ms";
        }

        public string MakeWebRequest(bool shouldRunLikeCrap)
        {
            string url = "https://www.google.com";

            var request = WebRequest.Create(url);
            using (var httpWebResponse = (HttpWebResponse)request.GetResponse())
            {
                var status = httpWebResponse.StatusDescription;
                using (var dataStream = httpWebResponse.GetResponseStream())
                using (var reader = new StreamReader(dataStream))
                {
                    var response = reader.ReadToEnd();
                    return $"Web Request - status {status}";
                }
            }
        }

        public string MakeHttpRequest(bool shouldRunLikeCrap)
        {
            string url = "https://www.yahoo.com";

            using (var client = new HttpClient())
            {
                var responseTask = client.GetAsync(url);
                responseTask.Wait();
                var result = responseTask.Result;
                return $"Http Request - status {result.StatusCode}";
            }
        }

        public void DoBusyWork(bool shouldRunLikeCrap)
        {
            var random = new Random();
            int minValueMs = 1000; // 1 second
            int maxValueMs = minValueMs * 3;

            if (shouldRunLikeCrap)
            {
                Console.WriteLine("(slow mode) You keep pushing me... boy.");
                minValueMs *= 2;
                maxValueMs *= 2;
            }

            int sleepingMs;
            for (int i = 0; i < 10; i++)
            {
                sleepingMs = random.Next(minValueMs, maxValueMs);
                Console.WriteLine($"Performing Task for {sleepingMs} ms");

                for (int j = 1; j < sleepingMs + 1; j++)
                {
                    int k = 1;
                    k *= j;
                    k /= j;
                    k += j;
                    k -= j;
                }
                Thread.Sleep(sleepingMs);
            }
        }

        public void onDebug()
        {
            OnStart(null);
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            var stopWatch = new Stopwatch();
            var random = new Random();
            bool shouldRunLikeCrap = false;

            var output = random.Next(1, 4);

            // 1/4 chance of crap run
            if (output == 4)
                shouldRunLikeCrap = true;

            stopWatch.Start();

            var output1 = MakeHttpRequest(shouldRunLikeCrap);
            log.Info(output1);
            var output2 = MakeWebRequest(shouldRunLikeCrap);
            log.Info(output2);
            var output3 = MakeProcessRequest(shouldRunLikeCrap);
            log.Info(output3);
            DoBusyWork(shouldRunLikeCrap);

            stopWatch.Stop();
            var crapString = (shouldRunLikeCrap ? " (crap)" : "");
            string message = $"OnTimer event - Job Finished{crapString}. Took {stopWatch.ElapsedMilliseconds} ms";
            eventLog1.WriteEntry(message, EventLogEntryType.Information, eventId++);
            log.Info(message);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        protected override void OnStart(string[] args)
        {
            var serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            var message = $"In OnStart{Environment.NewLine}";
            message += $"Launched from {Environment.CurrentDirectory}{Environment.NewLine}";
            message += $"Physical location {AppDomain.CurrentDomain.BaseDirectory}{Environment.NewLine}";
            message += $"AppContext.BaseDir {AppContext.BaseDirectory}{Environment.NewLine}";
            message += $"Runtime Call {Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)}";
            log.Info(message);

            OnTimer(this, null);
            timer.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            string message = "In OnStop.";
            log.Info(message);
            // Update the service state to Stopped.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }
    }
}
public enum ServiceState
{
    SERVICE_STOPPED = 0x00000001,
    SERVICE_START_PENDING = 0x00000002,
    SERVICE_STOP_PENDING = 0x00000003,
    SERVICE_RUNNING = 0x00000004,
    SERVICE_CONTINUE_PENDING = 0x00000005,
    SERVICE_PAUSE_PENDING = 0x00000006,
    SERVICE_PAUSED = 0x00000007,
}

[StructLayout(LayoutKind.Sequential)]
public struct ServiceStatus
{
    public int dwServiceType;
    public ServiceState dwCurrentState;
    public int dwControlsAccepted;
    public int dwWin32ExitCode;
    public int dwServiceSpecificExitCode;
    public int dwCheckPoint;
    public int dwWaitHint;
};
