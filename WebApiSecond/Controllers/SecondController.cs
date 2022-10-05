using System;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebApiSecond.EfCore;

namespace WebApiSecond.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SecondController : ControllerBase
    {
        private readonly ILogger<SecondController> _logger;
        private readonly WebApiSecondDbContext _dbContext;
        private readonly DaprClient _daprClient;
        private readonly HttpClient _httpClient;

        public SecondController(DaprClient daprClient, ILogger<SecondController> logger, WebApiSecondDbContext dbContext, HttpClient httpClient)
        {
            _daprClient = daprClient;
            _logger = logger;
            _dbContext = dbContext;
            _httpClient = httpClient;
        }

        [HttpGet("/migrate")]
        public async Task Migrate()
        {
            _logger.LogInformation("Migrating a db");

            try
            {
                await _dbContext.Database.MigrateAsync();
                _logger.LogInformation("Migration succeeded");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Migration failed");
            }
        }


        [HttpGet("/nameandsurname")]
        public async Task<string> GetNameAndSurname()
        {
            _logger.LogInformation("nameandsurname");

            _logger.LogInformation("HttpClient calls google.com");
            await _httpClient.GetStringAsync("https://google.com");

            JsonObject response = await _daprClient.InvokeMethodAsync<JsonObject>(HttpMethod.Get, "webapifirstraz", "generatename");
            response.TryGetPropertyValue("Name", out JsonNode node);
            string fetchedName = node.GetValue<string>();

            _logger.LogInformation("Fetched name: {Name}", fetchedName);

            string[] surnames = new string[] { "Smith", "Alvarez", "Kowalski" };
            Random random = new Random();
            string chosenSurname = surnames[random.Next(surnames.Length)];

            string nameAndSurname = fetchedName + " " + chosenSurname;
            _logger.LogInformation("Created name and surname: {NameAndSurname}", nameAndSurname);

            _ = await _dbContext.AddAsync<Person>(new Person() { Name = fetchedName, Surname = chosenSurname });
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Saved a Person to a DB: {NameAndSurname}", nameAndSurname);
            return nameAndSurname;
        }

    }
}