namespace TradeMcp.Core.Query;

public readonly record struct DateRange(DateTime Start, DateTime End)
{
    public string StartText => Start.ToString("yyyyMMdd");
    public string EndText => End.ToString("yyyyMMdd");

    public static DateRange Resolve(
        string? startDate,
        string? endDate,
        int? days,
        int? months,
        int defaultDays = 30)
    {
        var today = DateTime.Today;
        DateTime end;
        DateTime start;

        if (!string.IsNullOrWhiteSpace(endDate))
            end = Parse(endDate!);
        else
            end = today;

        if (!string.IsNullOrWhiteSpace(startDate))
        {
            start = Parse(startDate!);
        }
        else if (months is > 0)
        {
            start = end.AddMonths(-months.Value);
        }
        else if (days is > 0)
        {
            start = end.AddDays(-(days.Value - 1));
        }
        else if (!string.IsNullOrWhiteSpace(startDate) || !string.IsNullOrWhiteSpace(endDate))
        {
            // end only → defaultDays window ending at end
            start = end.AddDays(-(defaultDays - 1));
        }
        else
        {
            start = end.AddDays(-(defaultDays - 1));
        }

        if (start > end)
            (start, end) = (end, start);

        return new DateRange(start.Date, end.Date);
    }

    public IEnumerable<(DateTime Start, DateTime End)> Chunk(int maxDays)
    {
        if (maxDays <= 0) maxDays = 30;
        var cursor = Start;
        while (cursor <= End)
        {
            var chunkEnd = cursor.AddDays(maxDays - 1);
            if (chunkEnd > End) chunkEnd = End;
            yield return (cursor, chunkEnd);
            cursor = chunkEnd.AddDays(1);
        }
    }

    private static DateTime Parse(string text)
    {
        text = text.Trim();
        if (DateTime.TryParseExact(text, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var d))
            return d;
        if (DateTime.TryParse(text, out d))
            return d.Date;
        throw new ArgumentException($"invalid date '{text}', expected yyyyMMdd");
    }
}
