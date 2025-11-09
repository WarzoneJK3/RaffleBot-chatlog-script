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
        DateTime dateTime = DateTime.Parse(lineText.Substring(0, Constants.UtcTimeStampLength));
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

        if (previousMessage is RaffleFactMessage factMessage && !message.Contains(Constants.CommentIndicator))
        {
            return new RaffleFactExtensionMessage(dateTime, message, factMessage);
        }

        throw new InvalidCastException($"Cannot convert \"{message}\" into a known message type");
    }

    protected static AdditionalRewardType ParseAdditionalReward(string message)
    {
        const string AndAText = "and a ";

        int indexOfAndA = message.IndexOf(AndAText, StringComparison.Ordinal);
        if (indexOfAndA > 0)
        {
            string reward = message.Substring(indexOfAndA + AndAText.Length);

            if (reward.StartsWith(Constants.FogBuster, StringComparison.Ordinal))
            {
                return AdditionalRewardType.FogBusterPower;
            }

            if (reward.StartsWith(Constants.FreeCache, StringComparison.Ordinal))
            {
                return AdditionalRewardType.FreeCachePower;
            }
            
            if (reward.StartsWith(Constants.InspireMercenaries, StringComparison.Ordinal))
            {
                return AdditionalRewardType.InspireMercenariesPower;
            }
            
            if (reward.StartsWith(Constants.MarketRaid, StringComparison.Ordinal))
            {
                return AdditionalRewardType.MarketRaidPower;
            }

            if (reward.StartsWith(Constants.PoorArtifact, StringComparison.Ordinal))
            {
                return AdditionalRewardType.PoorArtifact;
            }

            if (reward.StartsWith(Constants.SuperChargeArmyCamp, StringComparison.Ordinal))
            {
                return AdditionalRewardType.SuperChargeArmyCampPower;
            }

            if (reward.StartsWith(Constants.SuperChargeMine, StringComparison.Ordinal))
            {
                return AdditionalRewardType.SuperchargeMinePower;
            }

            if (reward.StartsWith(Constants.TimeWarp, StringComparison.Ordinal))
            {
                return AdditionalRewardType.TimeWarpPower;
            }

            throw new InvalidCastException($"Cannot detect a known reward in \"{reward}\"");
        }

        return AdditionalRewardType.None;
    }

    protected RaffleVariety ParseRaffleVariety(string message)
    {
        if (message.Contains(Constants.Raffle, StringComparison.Ordinal))
        {
            return RaffleVariety.Raffle;
        }

        if (message.Contains(Constants.Waffle, StringComparison.Ordinal))
        {
            return RaffleVariety.Waffle;
        }

        if (message.Contains(Constants.Wafaffle, StringComparison.Ordinal))
        {
            return RaffleVariety.Wafaffle;
        }

        if (message.Contains(Constants.Ruthless, StringComparison.Ordinal))
        {
            return RaffleVariety.Ruthless;
        }

        throw new InvalidCastException($"Cannot detect a known raffle variety in \"{message}\"");
    }
}

public class RaffleStartMessage : RaffleMessage
{
    public int CoinPrice { get; }
    public AdditionalRewardType AdditionalReward { get; }
    public RaffleVariety RaffleVariety { get; }

    public RaffleStartMessage(DateTime dateTime, string message) : base(dateTime, message)
    {
        int index = message.IndexOf(' ', StringComparison.Ordinal);
        CoinPrice = int.Parse(message.Substring(0, index));
        AdditionalReward = ParseAdditionalReward(message);
        RaffleVariety = ParseRaffleVariety(message);
    }
}

public class RaffleEndMessage : RaffleMessage
{
    private static readonly Regex _nameAndCoinsRegex = new Regex(@"RAFFLE OVER: Congratulations to (.+) for winning (\d+) coins");

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
            throw new InvalidCastException($"Unable to parse end of raffle from \"{message}\"");
        }

        Winner = match.Groups[1].Value;
        Coins = int.Parse(match.Groups[2].Value);
        AdditionalReward = ParseAdditionalReward(message);

        int index = message.Substring(0, message.Length -1).LastIndexOf('!');
        NextRaffleVariety = ParseRaffleVariety(message.Substring(index));
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
        Success = message.Substring(index + 2).StartsWith(SuccessfulRaffleMessage);
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