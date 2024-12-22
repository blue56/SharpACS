using System;

public class DatePeriod
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public DatePeriod(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    public string GenerateKey(string pattern)
    {
        string periodString = ToPeriodString();
        return pattern.Replace("{period}", periodString)
                      .Replace("{start_date}", StartDate.ToString("yyyy-MM-dd"))
                      .Replace("{end_date}", EndDate.ToString("yyyy-MM-dd"));
    }

    public string ToPeriodString()
    {
        if (StartDate.Day == 1 && EndDate == StartDate.AddMonths(1).AddDays(-1))
        {
            return $"month:{StartDate:yyyy-MM}";
        }
        else if (StartDate == new DateTime(StartDate.Year, 1, 1) && EndDate == new DateTime(StartDate.Year, 12, 31))
        {
            return $"year:{StartDate:yyyy}";
        }
        else if (EndDate == DateTime.Now.Date && StartDate == EndDate.AddDays(-7))
        {
            return "last_7_days";
        }
        else if (EndDate == DateTime.Now.Date && StartDate == EndDate.AddDays(-30))
        {
            return "last_30_days";
        }
        return $"custom:{StartDate:yyyy-MM-dd},{EndDate:yyyy-MM-dd}";
    }
}

public class DatePeriodTranslator
{
    public static DatePeriod Translate(string period)
    {
        DateTime now = DateTime.Now;
        DateTime start;
        DateTime end;

        if (period.ToLower().StartsWith("month:"))
        {
            var parts = period.Split(":");
            if (parts.Length == 2 && DateTime.TryParseExact(parts[1], "yyyy-MM", null, System.Globalization.DateTimeStyles.None, out DateTime specificMonth))
            {
                start = new DateTime(specificMonth.Year, specificMonth.Month, 1);
                end = start.AddMonths(1).AddDays(-1);
                return new DatePeriod(start, end);
            }
            else
            {
                throw new ArgumentException("Invalid specific month format. Use 'month:yyyy-MM'.", nameof(period));
            }
        }

        switch (period.ToLower())
        {
            case "current_month":
                start = new DateTime(now.Year, now.Month, 1);
                end = start.AddMonths(1).AddDays(-1);
                break;

            case "previous_month":
                start = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
                end = start.AddMonths(1).AddDays(-1);
                break;

            case "current_year":
                start = new DateTime(now.Year, 1, 1);
                end = new DateTime(now.Year, 12, 31);
                break;

            case "previous_year":
                start = new DateTime(now.Year - 1, 1, 1);
                end = new DateTime(now.Year - 1, 12, 31);
                break;

            case "last_7_days":
                start = now.Date.AddDays(-7);
                end = now.Date;
                break;

            case "last_30_days":
                start = now.Date.AddDays(-30);
                end = now.Date;
                break;

            default:
                throw new ArgumentException("Unsupported period string.", nameof(period));
        }

        return new DatePeriod(start, end);
    }
}
