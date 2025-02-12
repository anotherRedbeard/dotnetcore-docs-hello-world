// File: CommandLineApp.cs
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace dotnetcoresample
{
    public class CommandLineApp
    {
        private readonly ILogger<CommandLineApp> _logger;

        public CommandLineApp(ILogger<CommandLineApp> logger)
        {
            _logger = logger;
        }

        public void Run()
        {
            _logger.LogInformation("CommandLineApp is running.");
            Console.WriteLine("Hello from CommandLineApp!");
            // Add your logic here
        }
    }
}