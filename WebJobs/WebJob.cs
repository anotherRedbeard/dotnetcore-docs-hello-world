// filepath: /Users/andrewredman/src/dotnet/dotnetcore-docs-hello-world/WebJob.cs
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;

namespace dotnetcoresample.WebJobs
{
    public class WebJob
    {
        public static void LogMessage([TimerTrigger("*/10 * * * * *")] TimerInfo timer, ILogger logger)
        {
            logger.LogInformation($"WebJob triggered at: {DateTime.Now}");
        }
    }
}