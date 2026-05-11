using AccuScraperWebWithReact.Server.Settings;

namespace AccuScraperWebWithReact.Server.Helpers
{
    public static class TelegramService
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        public static async Task SendMessageAsync(List<InsuranceData> data, TelegramSettings settings)
        {
            // Prepare the message
            if (data.Any())
            {
                var firstThree = data.Take(3).ToList();
                var messageTemplate = string.Join("\n\n", firstThree.Select((item, index) =>
                    $"Record {index + 1}:\n" +
                    $"    Account ID: {item.AccountId}\n" +
                    $"    Patient Name: {item.PatientName}\n" +
                    $"    Patient DOB: {item.PatientDOB}\n" +
                    $"    Gender: {item.PatientGender}\n" +
                    $"    Insurance Name: {item.InsuranceName}\n" +
                    $"    Insurance ID: {item.InsuranceId}\n" +
                    $"    Note: {item.Note}"));

                string message = Uri.EscapeDataString($"У тебе {data.Count} {GetInsuranceWordForm(data.Count)}" +
                    $"\n\n{messageTemplate}");

                var urls = new[]
                {
                    $"https://api.telegram.org/bot{settings.BotToken}/sendMessage?chat_id={settings.MyId}&text={message}",
                    $"https://api.telegram.org/bot{settings.BotToken}/sendMessage?chat_id={settings.TaniaId}&text={message}",
                    $"https://api.telegram.org/bot{settings.BotToken}/sendMessage?chat_id={settings.VovaId}&text={message}"
                };

                foreach (var url in urls)
                {
                    var response = await HttpClient.PostAsync(url, null);
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        public static async Task SendLogMessageAsync(string message, TelegramSettings settings)
        {
            var urls = new[]
            {
                $"https://api.telegram.org/bot{settings.LogBotToken}/sendMessage?chat_id={settings.MyId}&text={message}",
                //$"https://api.telegram.org/bot{settings.LogBotToken}/sendMessage?chat_id={settings.TaniaId}&text={message}",
                //$"https://api.telegram.org/bot{settings.LogBotToken}/sendMessage?chat_id={settings.VovaId}&text={message}"
            };

            foreach (var url in urls)
            {
                var response = await HttpClient.PostAsync(url, null);
                response.EnsureSuccessStatusCode();
            }
        }

        public static async Task SendErrorMessageAsync(string message, TelegramSettings settings)
        {
            var urls = new[]
            {
                $"https://api.telegram.org/bot{settings.LogBotToken}/sendMessage?chat_id={settings.MyId}&text={message}",
                //$"https://api.telegram.org/bot{settings.LogBotToken}/sendMessage?chat_id={settings.TaniaId}&text={message}",
                //$"https://api.telegram.org/bot{settings.LogBotToken}/sendMessage?chat_id={settings.VovaId}&text={message}"
            };

            foreach (var url in urls)
            {
                var response = await HttpClient.PostAsync(url, null);
                response.EnsureSuccessStatusCode();
            }
        }

        private static string GetInsuranceWordForm(int count)
        {
            if (count % 10 == 1 && count % 100 != 11)
                return "страховка потребує уваги!";
            else if (count % 10 >= 2 && count % 10 <= 4 && (count % 100 < 10 || count % 100 >= 20))
                return "страховки потребують уваги!";
            else
                return "страховок потребують уваги!";
        }
    }

    public class InsuranceData
    {
        public string AccountId { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public string PatientDOB { get; set; } = string.Empty;
        public string PatientGender { get; set; } = string.Empty;
        public string InsuranceName { get; set; } = string.Empty;
        public string InsuranceId { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
