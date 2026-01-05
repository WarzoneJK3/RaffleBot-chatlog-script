using System.Globalization;
using System.Text.RegularExpressions;
using RaffleLogParser.Enums;

namespace RaffleLogParser;

public class RaffleMessage
{
    protected RaffleMessage(DateTime timeStamp, string message)
    {
        TimeStamp = timeStamp;
        Message = message;
    }

    public DateTime TimeStamp { get; }
    public string Message { get; }

    public static RaffleMessage ParseLine(string lineText, RaffleMessage? previousMessage = null)
    {
        DateTime dateTime = DateTime.Parse(lineText.AsSpan(0, Constants.UtcTimeStampLength), null, DateTimeStyles.AssumeUniversal);
        string message = lineText.Substring(Constants.UtcTimeStampLength + 1).TrimEnd();

        if (message.StartsWith(Constants.RaffleEntryIndicator, StringComparison.Ordinal))
        {
            return new RaffleEntryMessage(dateTime, message);
        }

        if (message.EndsWith(Constants.RaffleStartIndicator, StringComparison.Ordinal))
        {
            return new RaffleStartMessage(dateTime, message);
        }

        if (message.StartsWith(Constants.RaffleEndIndicator, StringComparison.Ordinal))
        {
            return new RaffleEndMessage(dateTime, message);
        }

        if (message.StartsWith(Constants.RaffleFactIndicator, StringComparison.Ordinal))
        {
            return new RaffleFactMessage(dateTime, message);
        }

        // Some Raffle facts are so long they overflow into a second message
        if (previousMessage is RaffleFactMessage factMessage)
        {
            return new RaffleFactExtensionMessage(dateTime, message, factMessage);
        }

        throw new FormatException($"Cannot convert \"{message}\" into a known message type");
    }

    protected static AdditionalRewardType ParseAdditionalReward(string message)
    {
        const string AndAText = "and a ";

        int indexOfAndA = message.IndexOf(AndAText, StringComparison.Ordinal);

        if (indexOfAndA < 0)
        {
            return AdditionalRewardType.None;
        }

        ReadOnlySpan<char> trimmedMessage = message.AsSpan(indexOfAndA + AndAText.Length);

        foreach ((string rewardText, AdditionalRewardType rewardType) in AdditionalRewardTypes)
        {
            if (trimmedMessage.StartsWith(rewardText, StringComparison.Ordinal))
            {
                return rewardType;
            }
        }

        throw new FormatException($"Cannot detect a known reward in \"{message}\"");
    }

    protected RaffleVariety ParseRaffleVariety(string message, string expectedEndOfMessage)
    {
        ReadOnlySpan<char> trimmedMessage = message.AsSpan(0, message.Length - expectedEndOfMessage.Length);
        int startIndex = trimmedMessage.LastIndexOf(' ') + 1;

        if (Enum.TryParse(trimmedMessage.Slice(startIndex, trimmedMessage.Length - startIndex), true, out RaffleVariety raffleVariety))
        {
            return raffleVariety;
        }

        throw new FormatException($"Cannot detect a known raffle variety in \"{message}\"");
    }

    private static readonly (string MessageText, AdditionalRewardType Type)[] AdditionalRewardTypes =
    [
        (Constants.FogBuster, AdditionalRewardType.FogBusterPower),
        (Constants.FreeCache, AdditionalRewardType.FreeCachePower),
        (Constants.InspireMercenaries, AdditionalRewardType.InspireMercenariesPower),
        (Constants.MarketRaid, AdditionalRewardType.MarketRaidPower),
        (Constants.PoorArtifact, AdditionalRewardType.PoorArtifact),
        (Constants.SuperChargeArmyCamp, AdditionalRewardType.SuperChargeArmyCampPower),
        (Constants.SuperChargeMine, AdditionalRewardType.SuperchargeMinePower),
        (Constants.TimeWarp, AdditionalRewardType.TimeWarpPower)
    ];
}

public class RaffleStartMessage : RaffleMessage
{
    public int CoinPrice { get; }
    public AdditionalRewardType AdditionalReward { get; }
    public RaffleVariety RaffleVariety { get; }

    public RaffleStartMessage(DateTime dateTime, string message) : base(dateTime, message)
    {
        int index = message.IndexOf(' ', StringComparison.Ordinal);
        CoinPrice = int.Parse(message.AsSpan(0, index));
        AdditionalReward = ParseAdditionalReward(message);
        RaffleVariety = ParseRaffleVariety(message, " starting!");
    }
}

public class RaffleEndMessage : RaffleMessage
{
    private static readonly Regex _nameAndCoinsRegex = new Regex(@"^RAFFLE OVER: Congratulations to (.+) for winning (\d+) coins", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public bool HasWinner { get; }
    public string Winner { get; }
    public int Coins { get; }
    public AdditionalRewardType AdditionalReward { get; }
    public RaffleVariety NextRaffleVariety { get; }

    public RaffleEndMessage(DateTime dateTime, string message) : base(dateTime, message)
    {
        HasWinner = !message.StartsWith(Constants.FailedRaffleMessage, StringComparison.Ordinal);

        if (!HasWinner)
        {
            Winner = "";
            return;
        }

        Match match = _nameAndCoinsRegex.Match(message);

        if (!match.Success)
        {
            throw new FormatException($"Unable to parse end of raffle from \"{message}\"");
        }

        Winner = match.Groups[1].Value;
        Coins = int.Parse(match.Groups[2].Value);
        AdditionalReward = ParseAdditionalReward(message);
        NextRaffleVariety = ParseRaffleVariety(message, "!");
    }
}

public class RaffleEntryMessage : RaffleMessage
{
    private const string SuccessfulRaffleMessage = "OK";

    public string PlayerName { get; }
    public bool Success { get; }

    public RaffleEntryMessage(DateTime dateTime, string message) : base(dateTime, message)
    {
        int index = message.LastIndexOf(':');
        PlayerName = message.Substring(1, index - 1);
        Success = message.AsSpan(index + 2).StartsWith(SuccessfulRaffleMessage, StringComparison.Ordinal);
    }
}

public class RaffleFactMessage : RaffleMessage
{
    public string Fact { get; private set; }

    public RaffleFactMessage(DateTime dateTime, string message) : base(dateTime, message)
    {
        Fact = message.Substring(Constants.RaffleFactIndicator.Length);
    }
}

public class RaffleFactExtensionMessage : RaffleMessage
{
    public RaffleFactMessage RaffleFactMessage { get; }
    public string FullFactMessage => RaffleFactMessage.Message + ' ' + Message;

    public RaffleFactExtensionMessage(DateTime dateTime, string message, RaffleFactMessage factMessage) : base(dateTime, message)
    {
        RaffleFactMessage = factMessage;
    }
}
