namespace RaffleLogParser;

public static class Utility
{
    public static string TakeFirst(this string value, int count)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        return value.Length <= count ? value : value.Substring(0, count);
    }

    public static string ToQuotedString(this string? value)
    {
        const string DoubleQuote = "\"";
        string cleanedValue = value?.Replace(DoubleQuote, DoubleQuote + DoubleQuote) ?? "";
        return DoubleQuote + cleanedValue + DoubleQuote;
    }
    
    public static string ToQuotedString(this object? value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString("s").ToQuotedString();
        }
            
        return (value?.ToString()).ToQuotedString();
    }

    public static string BuildTsvString(params object?[] values)
    {
        return string.Join('\t', values.Select(ToQuotedString));
    }
}
