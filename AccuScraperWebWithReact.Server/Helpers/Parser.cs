using HtmlAgilityPack;

namespace AccuScraperWebWithReact.Server.Helpers
{
    public static class Parser
    {
        public static List<InsuranceData> Parse(string page)
        {
            var document = new HtmlDocument();
            document.LoadHtml(page);

            // Get all rows in the table
            var rows = document.DocumentNode.SelectNodes("//tr");
            var data = new List<InsuranceData>();

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    // Get all cells in the row
                    var cells = row.SelectNodes("td");
                    if (cells != null && cells.Count > 1)
                    {
                        // Find the status container and get its background color
                        var statusContainer = cells[1].SelectSingleNode(".//div");
                        if (statusContainer != null)
                        {
                            var bgColor = statusContainer.GetAttributeValue("style", "")
                                .Split(';')
                                .FirstOrDefault(s => s.Trim().StartsWith("background-color", StringComparison.OrdinalIgnoreCase))
                                ?.Split(':')[1]
                                ?.Trim();

                            // Check for yellow background
                            if (bgColor == "yellow") // Yellow in RGB format
                            //if (bgColor == "green") // Yellow in RGB format
                            {
                                var rowData = new InsuranceData
                                {
                                    AccountId = cells[2].InnerText.Trim(),
                                    PatientName = cells[3].InnerText.Trim(),
                                    PatientDOB = cells[4].InnerText.Trim(),
                                    PatientGender = cells[5].InnerText.Trim(),
                                    InsuranceName = cells[6].InnerText.Trim(),
                                    InsuranceId = cells[7].InnerText.Trim(),
                                    Note = cells[8].InnerText.Trim(),
                                };

                                data.Add(rowData);
                            }
                        }
                    }
                }
            }

            return data;
        }
    }
}
