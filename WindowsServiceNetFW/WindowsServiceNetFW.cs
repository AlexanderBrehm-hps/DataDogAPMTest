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

namespace WindowsServiceNetFW
{
    public partial class WindowsServiceNetFW : ServiceBase
    {
        private System.Timers.Timer timer;
        private int eventId = 1;

        public WindowsServiceNetFW()
        {
            InitializeComponent();

            string eventSource = "Hps-WindowsServiceNetFW";
            string logName = "Application";

            eventLog1 = new EventLog(logName);
            if (!EventLog.SourceExists(eventSource))
                EventLog.CreateEventSource(eventSource, logName);

            eventLog1.Source = eventSource;
            eventLog1.Log = logName;

            // Set up a timer that triggers every minute.
            timer = new System.Timers.Timer()
            {
                Interval = 60000 // 1 minute
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
        }

        public void onDebug()
        {
            OnStart(null);
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            var random = new Random();
            bool shouldRunLikeCrap = false;

            var output = random.Next(1, 4);

            // 1/4 chance of crap run
            if (output == 4)
                shouldRunLikeCrap = true;

            int minValueMs = 1000; // 1 second
            int maxValueMs = minValueMs * 3;

            if (shouldRunLikeCrap)
            {
                Console.WriteLine("(slow mode) You keep pushing me... boy.");
                minValueMs *= 2;
                maxValueMs *= 2;
            }

            var stopWatch = new Stopwatch();
            stopWatch.Start();
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

            stopWatch.Stop();
            var crapString = (shouldRunLikeCrap ? " (crap)" : "");
            eventLog1.WriteEntry($"OnTimer event - Job Finished{crapString}. Took {stopWatch.ElapsedMilliseconds} ms", EventLogEntryType.Information, eventId++);
        }

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("In OnStart.");
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            timer.Start();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop.");
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
