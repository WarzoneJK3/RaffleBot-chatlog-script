using RaffleLogParser.Enums;
using System.Diagnostics;

namespace RaffleLogParser;

public class Raffle
{
    public static readonly TimeSpan SnipedTimeSpan = TimeSpan.FromSeconds(5);

    public List<RaffleEntryMessage> Entries { get; }
    public int Coins { get; }
    public AdditionalRewardType AdditionalReward { get; }
    public RaffleVariety Variety { get; }
    public DateTime StartTime { get; }
    public DateTime EndTime { get; private set; }
    public TimeSpan Duration => HasEnded ? EndTime - StartTime : TimeSpan.Zero;
    public RaffleVariety? NextRaffleVariety { get; private set; }
    public bool HasEnded { get; private set; }
    public bool HasWinner { get; private set; }
    public string? WinnerName { get; private set; }
    public string? Fact { get; private set; }
    public bool IsSniped { get; private set; }
    public List<string>? PlayerNames { get; private set; }
    public int NumberOfPlayers { get; private set; }
    public int NumberOfPlayersJoined { get; private set; }
    public int NumberOfPlayersFailed => NumberOfPlayers - NumberOfPlayersJoined;
    public double WinChancePerJoinedPlayer => 1.0 / NumberOfPlayersJoined;
    
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
        if (HasEnded)
        {
            throw new InvalidOperationException("Cannot add ending to an already finished raffle");
        }

        HasWinner = endMessage.HasWinner;

        if (HasWinner && Coins != endMessage.Coins)
        {
            throw new InvalidOperationException($"This raffle is {Coins} coins, but a {endMessage.Coins} coins ending was added to it");
        }

        NextRaffleVariety = endMessage.NextRaffleVariety;
        EndTime = endMessage.TimeStamp;
        WinnerName = endMessage.Winner.TakeFirst(Constants.MaxLengthPlayerNameInRaffleMessage);

        UpdateEntryBasedProperties();
        HasEnded = true;
    }

    public void AddFact(RaffleFactMessage factMessage)
    {
        if (!HasEnded)
        {
            throw new InvalidOperationException("Cannot add facts to an unfinished raffle");
        }

        if (Fact != null)
        {
            throw new InvalidOperationException("Cannot add multiple facts to the same raffle");
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
        List<RaffleEntryMessage> entries = Entries.DistinctBy(e => e.PlayerName).ToList();
        PlayerNames = entries.Select(p => p.PlayerName).ToList();
        NumberOfPlayers = entries.Count;
        NumberOfPlayersJoined = entries.Count(x => x.Success);
        IsSniped = HasWinner && entries.All(e => EndTime - e.TimeStamp <= SnipedTimeSpan);
    }
}
