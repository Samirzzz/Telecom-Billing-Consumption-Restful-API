namespace TelecomBilling.Api.Utils
{
    public static class MonthFormatHelper
    {
        public static string? NormalizeMonthFormat(string? month)
        {
            if (string.IsNullOrWhiteSpace(month))
                return null;

            month = month.Trim().ToLower();

            if (System.Text.RegularExpressions.Regex.IsMatch(month, @"^\d{4}-\d{2}$"))
            {
                return month;
            }

            var monthNames = new Dictionary<string, string>
            {
                { "january", "01" }, { "jan", "01" },
                { "february", "02" }, { "feb", "02" },
                { "march", "03" }, { "mar", "03" },
                { "april", "04" }, { "apr", "04" },
                { "may", "05" },
                { "june", "06" }, { "jun", "06" },
                { "july", "07" }, { "jul", "07" },
                { "august", "08" }, { "aug", "08" },
                { "september", "09" }, { "sep", "09" },
                { "october", "10" }, { "oct", "10" },
                { "november", "11" }, { "nov", "11" },
                { "december", "12" }, { "dec", "12" }
            };

            var parts = month.Split('-', '/', ' ');
            if (parts.Length >= 2)
            {
                var monthPart = parts[0].Trim();
                var yearPart = parts[1].Trim();

                if (monthNames.TryGetValue(monthPart, out var monthNumber))
                {
                    if (int.TryParse(yearPart, out var year) && year >= 2000 && year <= 2100)
                    {
                        return $"{year}-{monthNumber}";
                    }
                }
            }
            else if (parts.Length == 1)
            {
                if (monthNames.TryGetValue(parts[0], out var monthNumber))
                {
                    var currentYear = DateTime.UtcNow.Year;
                    return $"{currentYear}-{monthNumber}";
                }
            }

            return null;
        }

        public static DateTime ParseMonthToStartDate(string month)
        {
            var normalizedMonth = NormalizeMonthFormat(month);
            if (string.IsNullOrEmpty(normalizedMonth))
            {
                throw new ArgumentException($"Invalid month format: '{month}'. Expected format: YYYY-MM (e.g., 2024-10)");
            }

            try
            {
                return DateTime.ParseExact($"{normalizedMonth}-01", "yyyy-MM-dd", null);
            }
            catch (FormatException)
            {
                throw new ArgumentException($"Invalid month format: '{month}'. Expected format: YYYY-MM (e.g., 2024-10)");
            }
        }
    }
}
