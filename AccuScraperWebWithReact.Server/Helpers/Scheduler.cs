namespace AccuScraperWebWithReact.Server.Helpers
{
    public class Scheduler : BackgroundService
    {
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(2);
        private readonly TimeZoneInfo _timeZone;
        private readonly ScraperSettings _scraperSettings;
        private readonly TelegramSettings _telegramSettings;

        public Scheduler(IConfiguration configuration) 
        {
            _timeZone = TimeZoneInfo.FindSystemTimeZoneById("E. Europe Standard Time");
            _scraperSettings = configuration.GetSection("Scraper").Get<ScraperSettings>() ?? new ScraperSettings();
            _telegramSettings = configuration.GetSection("Telegram").Get<TelegramSettings>() ?? new TelegramSettings();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await TelegramService.SendLogMessageAsync("message from scheduled app", _telegramSettings);

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, _timeZone);
                bool isWithinSchedule = false;

                // Check each day's specific time window
                if (now.DayOfWeek == DayOfWeek.Monday && now.Hour >= 18) // Monday 6 PM to midnight
                {
                    isWithinSchedule = true;
                }
                else if (now.DayOfWeek == DayOfWeek.Tuesday && now.Hour < 3) // Tuesday midnight to 3 AM
                {
                    isWithinSchedule = true;
                }
                else if (now.DayOfWeek == DayOfWeek.Tuesday && now.Hour >= 18) // Tuesday 6 PM to midnight
                {
                    isWithinSchedule = true;
                }
                else if (now.DayOfWeek == DayOfWeek.Wednesday && now.Hour < 3) // Wednesday midnight to 3 AM
                {
                    isWithinSchedule = true;
                }
                else if (now.DayOfWeek == DayOfWeek.Wednesday && now.Hour >= 18) // Wednesday 6 PM to midnight
                {
                    isWithinSchedule = true;
                }
                else if (now.DayOfWeek == DayOfWeek.Thursday && now.Hour < 3) // Thursday midnight to 3 AM
                {
                    isWithinSchedule = true;
                }
                else if (now.DayOfWeek == DayOfWeek.Thursday && now.Hour >= 18) // Thursday 6 PM to midnight
                {
                    isWithinSchedule = true;
                }
                else if (now.DayOfWeek == DayOfWeek.Friday && now.Hour < 3) // Friday midnight to 3 AM
                {
                    isWithinSchedule = true;
                }
                else if (now.DayOfWeek == DayOfWeek.Friday && now.Hour >= 18) // Friday 6 PM to midnight
                {
                    isWithinSchedule = true;
                }
                else if (now.DayOfWeek == DayOfWeek.Saturday && now.Hour < 3) // Saturday midnight to 3 AM
                {
                    isWithinSchedule = true;
                }

                if (isWithinSchedule)
                {
                    await TelegramService.SendLogMessageAsync(now.ToString("dd.MM.yyyy HH:mm:ss"), _telegramSettings);

                    try
                    {
                        var scrapedPage = await Scraper.ScrapePage(_scraperSettings);
                        var parsedData = Parser.Parse(scrapedPage);

                        await TelegramService.SendMessageAsync(parsedData, _telegramSettings);
                    }
                    catch (Exception ex)
                    {
                        await TelegramService.SendLogMessageAsync(ex.Message, _telegramSettings);
                    }
                }

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }
    }
}
