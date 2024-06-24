using System;
using System.Diagnostics;
using System.Threading;
using System.Web.Http;

namespace WebApplicationNetFW.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        //public string Get()
        //{
        //    return Get("");
        //}

        /*
         * http://localhost/api/values
         * http://localhost/api/values?mode=crap
         */
        // GET api/values/
        // GET api/values?mode=crap
        public string Get(string mode = "")
        {
            bool shouldRunLikeCrap = false;

            if (!string.IsNullOrWhiteSpace(mode) && mode == "crap")
                shouldRunLikeCrap = true;

            var random = new Random();

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
            return $"Job Finished{crapString}. Took {stopWatch.ElapsedMilliseconds} ms";
        }
    }
}
