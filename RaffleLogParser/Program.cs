using System.Diagnostics;
using System.Text;
using RaffleLogParser;

const string BinPathToFileDirectory = "../../../../";

RaffleLogFileParser.ParseFile(BinPathToFileDirectory + "RaffleBot chat log.txt");

//List<Raffle> raffles = RaffleLogFileParser.GetRaffles(new DateTime(0)).ToList();
List<Raffle> raffles = RaffleLogFileParser.Raffles;

int wonRaffles = raffles.Count(r => r.HasWinner);

Player playerJk3 = Player.Players["JK_3"];

List<Player> luckyPlayers = Player.Players.Values.OrderByDescending(p => p.Luck).ToList();
List<Raffle> party = raffles.Where(r => r.Duration.TotalSeconds > 60).OrderByDescending(r => r.Duration).ToList();
List<Raffle> sniped = raffles.FindAll(r => r.WasSniped);

List<(string Name, double CoinsWon, double CoinsExpected, double ExcessCoins)> excessCoins =
    Player.Players.Where(p => p.Value.RafflesJoined > 450)
        .Select(p =>
        {
            Player player = p.Value;
            double won = player.CoinsWon;
            double expected = player.CoinsExpected;
            return (Name: p.Key, CoinsWon: won, CoinsExpected: expected, ExcessCoins: won - expected);
        })
        .OrderByDescending(x => x.ExcessCoins)
        .ToList();

Debugger.Break();

//processStreaksPerRaffle(true);
//processStreaksPerPlayer(true);

void processStreaksPerRaffle(bool exportToCsv)
{
    const int InitialStreakCount = 1;

    List<int> streaks = new(raffles.Count);
    string? lastWinner = null;
    int currentStreak = InitialStreakCount;

    foreach (Raffle raffle in raffles)
    {
        if (!raffle.HasWinner)
        {
            streaks.Add(0);
            continue;
        }

        string winner = raffle.WinnerName!;

        if (winner == lastWinner)
        {
            currentStreak++;
        }
        else
        {
            currentStreak = InitialStreakCount;
        }

        streaks.Add(currentStreak);
        lastWinner = winner;
    }

    int maxStreak = streaks.Max();
    int indexOfStreak = streaks.IndexOf(maxStreak);
    int countOfSameStreak = streaks.Count(x => x == maxStreak);
    Raffle streakRaffle = raffles[indexOfStreak];

    if (exportToCsv)
    {
        StringBuilder sb = new StringBuilder(capacity: raffles.Count * 75);
        sb.AppendLine("sep=,");
        sb.AppendLine("\"WinnerName\",\"CoinPrice\",\"Streak\",\"EndTime\",\"IdlePrice\",\"HasWinner\"");

        for (int index = 0; index < raffles.Count; index++)
        {
            Raffle raffle = raffles[index];
            int streak = streaks[index];
            sb.AppendLine($"\"{raffle.WinnerName}\",\"{raffle.Coins}\",\"{streak}\",\"{raffle.EndTime:s}\",\"{raffle.AdditionalReward}\",\"{raffle.HasWinner}\"");
        }

        File.WriteAllText(BinPathToFileDirectory + @"Files\RaffleData.csv", sb.ToString());
    }
}

void processStreaksPerPlayer(bool exportToCsv)
{
    Dictionary<string, int> streakPerPlayer = new Dictionary<string, int>();

    foreach ((string playerName, Player player) in Player.Players)
    {
        const int InitialStreakCount = 0;

        int maxStreak = InitialStreakCount;
        int currentStreak = InitialStreakCount;

        foreach (Raffle raffle in raffles)
        {
            if (!raffle.HasWinner) continue;

            string winnerName = raffle.WinnerName!;

            if (winnerName == playerName)
            {
                currentStreak++;
            }
            else
            {
                currentStreak = InitialStreakCount;
            }

            if (currentStreak > maxStreak)
            {
                maxStreak = currentStreak;
            }
        }

        streakPerPlayer.Add(playerName, maxStreak);
    }

    if (exportToCsv)
    {
        StringBuilder sb = new StringBuilder(capacity: raffles.Count * 75);
        sb.AppendLine("sep=,");
        sb.AppendLine("\"Player\",\"Streak\",\"RafflesWon\",\"RafflesJoined\",\"ExpectedRafflesWon\",\"CoinsWon\",\"ExpectedCoinsWon\"");

        foreach ((string playerName, Player player) in Player.Players)
        {
            int streak = streakPerPlayer[playerName];
            sb.AppendLine($"\"{playerName}\",\"{streak}\",\"{player.RafflesWon}\",\"{player.RafflesJoined}\",\"{player.RafflesWonExpected}\",\"{player.CoinsWon}\",\"{player.CoinsExpected}\"");
        }

        File.WriteAllText(BinPathToFileDirectory + @"Files\PlayerData.csv", sb.ToString());
    }
}