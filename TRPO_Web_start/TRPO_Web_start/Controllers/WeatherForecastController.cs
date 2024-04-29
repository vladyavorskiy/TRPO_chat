using Microsoft.AspNetCore.Mvc;

namespace TRPO_Web_start.Controllers
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

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Today.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
        [HttpGet("GetByDate/{date:datetime}")]
        public IActionResult Get(DateTime date)
        {
            _logger.LogDebug("Get weather method with arg {date}", date);
            if (date < DateTime.Today)
            {
                _logger.LogWarning("Bad request, date = {date}", date);
                return  BadRequest("date must in future");
            }

            var result = Ok(Get().FirstOrDefault(x => x.Date == date));

            _logger.LogDebug("Get weather result {@result}", result);

            return result;
        }
    }
}
