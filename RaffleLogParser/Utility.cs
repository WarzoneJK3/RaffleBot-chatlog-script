namespace RaffleLogParser;

public static class Utility
{
    public static string TakeFirst(this string value, int count)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        return value.Length <= count ? value : value.Substring(0, count);
    }

    public static string StrToCsv(this string? value)
    {
        const string DoubleQuote = "\"";
        string cleanedValue = value?.Replace(DoubleQuote, DoubleQuote + DoubleQuote) ?? "";
        return DoubleQuote + cleanedValue + DoubleQuote;
    }

    public static string ObjToCsv(this object? value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("s").StrToCsv();
        }

        return (value?.ToString()).StrToCsv();
    }

    public static string BuildCsvString(params object?[] values)
    {
        return string.Join(',', values.Select(ObjToCsv));
    }
}
