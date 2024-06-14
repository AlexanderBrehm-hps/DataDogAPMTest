using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceNetCore
{
    public sealed class MyService
    {
        public void MakeWebCall(bool shouldRunLikeCrap)
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
                    Console.WriteLine($"Web Call - status {status}");
                }
            }
        }

        public string DoIntensiveWork()
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

            return $"OnTimer event - Job Finished{crapString}. Took {stopWatch.ElapsedMilliseconds} ms";
        }
    }
}
