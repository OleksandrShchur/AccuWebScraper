using System.Net;
using System.Text.Json;
using HtmlAgilityPack;

namespace AccuScraperWebWithReact.Server.Helpers
{
    public static class Scraper
    {
        private const string COOKIES_FILE = "cookies.json";

        public static async Task<string> ScrapePage(ScraperSettings settings)
        {
            try
            {
                // Initialize HttpClient with cookie support
                var cookieContainer = new CookieContainer();
                using var handler = new HttpClientHandler { CookieContainer = cookieContainer, UseCookies = true };
                using var client = new HttpClient(handler);

                // Set headers to mimic a browser
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Referer", settings.LoginUrl);

                // Step 1: Get the login page to extract CSRF token
                var loginPageResponse = await client.GetAsync(settings.LoginUrl);
                loginPageResponse.EnsureSuccessStatusCode();
                var loginPageHtml = await loginPageResponse.Content.ReadAsStringAsync();

                // Parse HTML to find CSRF token
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(loginPageHtml);
                var csrfTokenNode = htmlDoc.DocumentNode.SelectSingleNode("//input[@name='__RequestVerificationToken']");
                var csrfToken = csrfTokenNode?.Attributes["value"]?.Value;

                // Step 2: Prepare login form data
                var formData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("Input.Email", settings.Email),
                    new KeyValuePair<string, string>("Input.Password", settings.Password),
                    new KeyValuePair<string, string>("__RequestVerificationToken", csrfToken ?? "")
                });

                // Step 3: Perform login
                var loginResponse = await client.PostAsync(settings.LoginUrl, formData);
                loginResponse.EnsureSuccessStatusCode();

                // Check if login was successful
                var loginResponseUrl = loginResponse.RequestMessage?.RequestUri?.ToString() ?? string.Empty;
                if (loginResponseUrl.Contains("/Account/Login"))
                {
                    return $"Login failed. Check credentials or form data. Response: {await loginResponse.Content.ReadAsStringAsync()}";
                }

                // Step 4: Save cookies
                var cookies = cookieContainer.GetCookies(new Uri(settings.LoginUrl));
                var cookieList = cookies.Cast<Cookie>().Select(c => new
                {
                    c.Name,
                    c.Value,
                    c.Domain,
                    c.Path,
                    c.Expires,
                    c.Secure,
                    c.HttpOnly
                }).ToList();
                File.WriteAllText(COOKIES_FILE, JsonSerializer.Serialize(cookieList, new JsonSerializerOptions { WriteIndented = true }));

                // Step 5: Access the target page
                var targetResponse = await client.GetAsync(settings.TargetUrl);
                targetResponse.EnsureSuccessStatusCode();
                var targetHtml = await targetResponse.Content.ReadAsStringAsync();

                // Step 6: Parse and extract specific data (optional)
                htmlDoc.LoadHtml(targetHtml);
                var dataNodes = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'some-class')]"); // Adjust selector

                // Step 7: Return full page content
                return targetHtml;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
