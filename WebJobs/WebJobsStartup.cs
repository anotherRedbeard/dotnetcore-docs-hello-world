// filepath: /Users/andrewredman/src/dotnet/dotnetcore-docs-hello-world/WebJobsStartup.cs
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

[assembly: WebJobsStartup(typeof(dotnetcoresample.WebJobs.WebJobsStartup))]
namespace dotnetcoresample.WebJobs
{
    public class WebJobsStartup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder)
        {
            builder.Services.AddLogging();
        }
    }
}