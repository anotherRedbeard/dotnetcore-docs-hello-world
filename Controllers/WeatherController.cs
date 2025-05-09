using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using dotnetcoresample.Models;
using Microsoft.Extensions.Logging;

namespace dotnetcoresample.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeatherController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherController> _logger;

        public WeatherController(ILogger<WeatherController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{zipCode}")]
        public ActionResult<WeatherForecast> Get(string zipCode)
        {
            try
            {
                if (string.IsNullOrEmpty(zipCode) || zipCode.Length != 5 || !zipCode.All(char.IsDigit))
                {
                    _logger.LogWarning("Invalid zipcode format: {ZipCode}", zipCode);
                    return BadRequest("ZipCode must be a valid 5-digit US postal code");
                }
                
                _logger.LogInformation("Generating weather data for zipcode: {ZipCode}", zipCode);
                
                // Generate random weather data based on zipCode
                var rng = new Random(int.Parse(zipCode.Substring(0, 3)));
                
                var forecast = new WeatherForecast
                {
                    Date = DateTime.Now,
                    TemperatureC = rng.Next(-20, 55),
                    ZipCode = zipCode,
                    Summary = Summaries[rng.Next(Summaries.Length)]
                };
                
                return forecast;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weather for zipcode: {ZipCode}", zipCode);
                return StatusCode(500, "An error occurred while processing your request");
            }
        }
    }
}