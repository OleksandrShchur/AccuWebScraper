using AccuScraperWebWithReact.Server.Helpers;
using AccuScraperWebWithReact.Server.Settings;
using Microsoft.AspNetCore.Mvc;

namespace AccuScraperWebWithReact.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScraperController : ControllerBase
    {
        private readonly ScraperSettings _scraperSettings;
        private readonly TelegramSettings _telegramSettings;

        public ScraperController(IConfiguration configuration)
        {
            _scraperSettings = configuration.GetSection("Scraper").Get<ScraperSettings>() ?? new ScraperSettings();
            _telegramSettings = configuration.GetSection("Telegram").Get<TelegramSettings>() ?? new TelegramSettings();
        }

        /// <summary>
        /// To execute https://localhost:7090/api/Scraper
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> ScrapePage()
        {
            try
            {
                var kyivTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kiev");
                var kyivTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, kyivTimeZone);

                if (kyivTime.Hour >= 3 && kyivTime.Hour <= 17)
                    return Ok("Time is not in range 6pm - 3am Monday-Saturday.");

                var scrapedPage = await Scraper.ScrapePage(_scraperSettings);
                var parsedData = Parser.Parse(scrapedPage);

                await TelegramService.SendMessageAsync(parsedData, _telegramSettings);
                await TelegramService.SendLogMessageAsync(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), _telegramSettings);

                return Ok(parsedData);
            }
            catch (Exception ex)
            {
                await TelegramService.SendErrorMessageAsync($"Contact @OS_immortal ASAP man! Error: {ex.Message}", _telegramSettings);

                return BadRequest(ex.Message);
            }
        }
    }
}
