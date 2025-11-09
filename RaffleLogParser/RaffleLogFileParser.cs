using System.Diagnostics;

namespace RaffleLogParser;

public static class RaffleLogFileParser
{
    public static readonly List<Raffle> Raffles = new();
    public static readonly List<RaffleMessage> RaffleMessages = new();

    public static void ParseFile(string path)
    {
        Stopwatch timer = Stopwatch.StartNew();
        
        IEnumerable<string> lines = File.ReadLines(path);
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(Constants.CommentIndicator))
            {
                continue;
            }

            RaffleMessage message = RaffleMessage.ParseLine(line, RaffleMessages.LastOrDefault());
            RaffleMessages.Add(message);
        }

        Raffle activeRaffle = null!;
        foreach (RaffleMessage raffleMessage in RaffleMessages)
        {
            switch (raffleMessage)
            {
                case RaffleStartMessage raffleStartMessage:
                    activeRaffle = new Raffle(raffleStartMessage);
                    Raffles.Add(activeRaffle);
                    break;

                case RaffleEntryMessage raffleEntryMessage:
                    activeRaffle.AddEntry(raffleEntryMessage);
                    break;

                case RaffleEndMessage raffleEndMessage:
                    activeRaffle.AddEnd(raffleEndMessage);
                    break;

                case RaffleFactMessage raffleFactMessage:
                    activeRaffle.AddFact(raffleFactMessage);
                    break;

                case RaffleFactExtensionMessage raffleFactExtensionMessage:
                    activeRaffle.AddFact(raffleFactExtensionMessage);
                    break;
            }
        }

        foreach (Raffle raffle in Raffles)
        {
            if (raffle.PlayerNames == null) continue;

            foreach (string playerName in raffle.PlayerNames)
            {
                Player.GetPlayer(playerName).AddRaffle(raffle);
            }
        }

        timer.Stop();
        Console.WriteLine($"Took {timer.Elapsed} to process {Raffles.Count} raffles and {Player.Players.Count} players");
    }

    public static IEnumerable<Raffle> GetRaffles(DateTime afterDateTime, DateTime? beforeDateTime = null)
    {
        if (beforeDateTime == null)
        {
            return Raffles.Where(r => r.EndTime >= afterDateTime);
        }

        return Raffles.Where(r => r.EndTime >= afterDateTime && r.EndTime <= beforeDateTime);
    }
}
