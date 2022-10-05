using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebApiFirst.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet("/myendpoint")]
        public IEnumerable<WeatherForecast> Get()
        {
            _logger.LogInformation("MyEndpoint called run at {DateTime}", DateTime.Now);
            Random rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("myendpoint")]
        public IEnumerable<WeatherForecast> Get2()
        {
            _logger.LogInformation("MyEndpoint called run at {DateTime}", DateTime.Now);
            Random rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        public record ResponseName(string Name);

        [HttpGet("/generatename")]
        public ActionResult<ResponseName> GenerateName()
        {
            _logger.LogInformation("GenerateName");
            
            string[] names = new string[] { "John", "Anna", "Emily", "Jack", "Adam" };
            Random rng = new();

            string chosenName = names[rng.Next(names.Length)];

            _logger.LogInformation("Chosen: {Name}", chosenName);
            return new ResponseName(chosenName);
        }

        [HttpPost("/cronjob")]
        public ActionResult PostCron()
        {
            _logger.LogInformation("Cron job run at {DateTime}", DateTime.Now);
            return Ok();
        }
    }
}
