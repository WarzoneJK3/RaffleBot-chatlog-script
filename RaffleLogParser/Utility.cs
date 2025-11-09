namespace RaffleLogParser;

public static class Utility
{
    public static string TakeFirst(this string value, int count)
    {
        ArgumentNullException.ThrowIfNull(value);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);

        return value.Length <= count ? value : value.Substring(0, count);
    }
}
