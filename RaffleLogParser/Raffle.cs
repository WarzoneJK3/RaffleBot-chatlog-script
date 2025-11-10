using RaffleLogParser.Enums;
using System.Reflection.Metadata;

namespace RaffleLogParser;

public class Raffle
{
    public static readonly TimeSpan SnipedTimeSpan = TimeSpan.FromSeconds(5);

    public List<RaffleEntryMessage> Entries { get; }
    public int Coins { get; }
    public AdditionalRewardType AdditionalReward { get; }
    public RaffleVariety Variety { get; }
    public DateTime StartTime { get; }

    public bool HasEnded { get; private set; }
    public bool HasWinner { get; private set; }
    public DateTime EndTime { get; private set; }
    public RaffleVariety? NextRaffleVariety { get; private set; }
    public string? WinnerName { get; private set; }
    public bool WasSniped { get; private set; }
    
    public string? Fact { get; private set; }

    
    public int NumberOfPlayers { get; private set; }
    public int NumberOfPlayersJoined { get; private set; }
    public int NumberOfPlayersFailed { get; private set; }
    public TimeSpan Duration { get; private set; }
    public List<string>? PlayerNames { get; private set; }
    public double WinChancePerJoinedPlayer { get; private set; }
    
    public Raffle(int coins, AdditionalRewardType additionalReward, RaffleVariety variety, DateTime startTime)
    {
        Coins = coins;
        AdditionalReward = additionalReward;
        Variety = variety;
        StartTime = startTime;
        Entries = new List<RaffleEntryMessage>();
    }

    public Raffle(RaffleStartMessage startMessage) : this(startMessage.CoinPrice, startMessage.AdditionalReward, startMessage.RaffleVariety, startMessage.TimeStamp)
    {
    }

    public void AddEntry(RaffleEntryMessage entry)
    {
        if (HasEnded)
        {
            // Sometimes there is a situation where the last entry in the raffle gets logged during or after the fact
            TimeSpan difference = entry.TimeStamp - EndTime;
            if (difference.Duration() > TimeSpan.FromSeconds(1))
            {
                throw new InvalidOperationException("Cannot add new entry messages to a finished raffle");
            }

            UpdateEntryBasedProperties();
        }

        Entries.Add(entry);
    }

    public void AddEnd(RaffleEndMessage endMessage)
    {
        HasWinner = endMessage.HasWinner;

        if (HasWinner && Coins != endMessage.Coins)
        {
            throw new InvalidOperationException($"This raffle is {Coins} coins, but a {endMessage.Coins} coins ending was added to it");
        }

        NextRaffleVariety = endMessage.NextRaffleVariety;
        EndTime = endMessage.TimeStamp;
        WinnerName = endMessage.Winner.TakeFirst(Constants.MaxLengthPlayerNameInRaffleMessage);

        Duration = EndTime - StartTime;
        UpdateEntryBasedProperties();
        HasEnded = true;
    }

    public void AddFact(RaffleFactMessage factMessage)
    {
        if (!HasEnded)
        {
            throw new InvalidOperationException("Cannot add facts to an unfinished raffle");
        }

        Fact = factMessage.Fact;
    }

    public void AddFact(RaffleFactExtensionMessage factExtensionMessage)
    {
        if (!HasEnded)
        {
            throw new InvalidOperationException("Cannot add facts to an unfinished raffle");
        }

        if (Fact == null)
        {
            throw new InvalidOperationException("Cannot add fact extension to a raffle without a fact");
        }

        Fact = factExtensionMessage.FullFactMessage;
    }

    private void UpdateEntryBasedProperties()
    {
        List<RaffleEntryMessage> players = Entries.DistinctBy(e => e.PlayerName).ToList();
        PlayerNames = players.Select(p => p.PlayerName).ToList();
        NumberOfPlayers = players.Count;
        NumberOfPlayersFailed = players.Count(x => !x.Success);
        NumberOfPlayersJoined = players.Count(x => x.Success);
        WinChancePerJoinedPlayer = 1.0 / NumberOfPlayersJoined;
        WasSniped = HasWinner && players.All(p => EndTime - p.TimeStamp <= SnipedTimeSpan);
    }
}
