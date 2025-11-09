namespace RaffleLogParser;

public class Player
{
    public static readonly Dictionary<string, Player> Players = new Dictionary<string, Player>();

    public string Name { get; }
    public List<Raffle> Raffles { get; } = new List<Raffle>();

    public int RafflesTotal => Raffles.Count;
    public int RafflesWon { get; private set; }
    public int RafflesJoined { get; private set; }
    public int RafflesLost { get; private set; }
    public double RafflesWonExpected { get; private set; }
    public double WinLuck => RafflesWon / RafflesWonExpected;
    public int CoinsWon { get; private set; }
    public double CoinsExpected { get; private set; }

    public double CoinLuck => CoinsWon / CoinsExpected;
    public double Luck => WinLuck - 1;

    public Player(string name)
    {
        Players.Add(name, this);
        Name = name;
    }

    public static Player GetPlayer(string name)
    {
        if (Players.TryGetValue(name, out Player? player))
        {
            return player;
        }

        return new Player(name);
    }

    public void AddRaffle(Raffle raffle)
    {
        RaffleEntryMessage? entry = raffle.Entries.Find(e => e.PlayerName == Name);

        if (entry == null)
        {
            throw new InvalidOperationException($"Player {Name} did not join the raffle you are trying to add");
        } 
        
        Raffles.Add(raffle);

        if (entry.Success)
        {
            RafflesJoined++;
            CoinsExpected += raffle.Coins * raffle.WinChancePerJoinedPlayer;
            RafflesWonExpected += raffle.WinChancePerJoinedPlayer;

            if (raffle.WinnerName == Name)
            {
                RafflesWon++;
                CoinsWon += raffle.Coins;
            }
            else
            {
                RafflesLost++;
            }
        }
    }
}
