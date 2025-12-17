using System.Diagnostics;
using System.Text;

namespace RaffleLogParser;

public class RaffleLogFile
{
    public readonly List<Raffle> Raffles = new();
    public readonly List<RaffleMessage> RaffleMessages = new();

    public RaffleLogFile(string filename)
    {
        ParseFile(filename);
    }

    private void ParseFile(string path)
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

    public IEnumerable<Raffle> GetRaffles(DateTime afterDateTime, DateTime? beforeDateTime = null)
    {
        if (beforeDateTime == null)
        {
            return Raffles.Where(r => r.EndTime >= afterDateTime);
        }

        return Raffles.Where(r => r.EndTime >= afterDateTime && r.EndTime <= beforeDateTime);
    }

    public void WriteRafflesToCsv(string filePath)
    {
        StringBuilder sb = new StringBuilder(Raffles.Count * 200);
        sb.AppendLine("sep=,");
        sb.AppendLine("Coins,AdditionalReward,Variety,StartTime,HasEnded,HasWinner,EndTime,NextRaffleVariety,WinnerName,WasSniped,Fact,NumberOfPlayers,NumberOfPlayersJoined,NumberOfPlayersFailed,Duration,WinChancePerJoinedPlayer");
        
        foreach (Raffle r in Raffles) 
        {
            sb.AppendLine(Utility.BuildCsvString(r.Coins, r.AdditionalReward, r.Variety, r.StartTime, r.HasEnded, r.HasWinner, r.EndTime, r.NextRaffleVariety, r.WinnerName, r.WasSniped, r.NumberOfPlayers, r.NumberOfPlayersJoined, r.NumberOfPlayersFailed, r.Duration, r.WinChancePerJoinedPlayer));
        }

        File.WriteAllText(filePath, sb.ToString());
    }
}
