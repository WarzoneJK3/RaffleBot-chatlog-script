namespace RaffleLogParser;

public static class Constants
{
    public const char CommentIndicator = '#';
    public const int UtcTimeStampLength = 19;
    
    public const string RaffleStartIndicator = "affle starting!";
    public const string RaffleEntryIndicator = "@";
    public const string RaffleEndIndicator = "RAFFLE OVER:";
    public const string RaffleFactIndicator = "Warzone Fact:";

    public const string FogBuster = "Fog Buster";
    public const string FreeCache = "Free Cache";
    public const string InspireMercenaries = "Inspire Mercenaries";
    public const string MarketRaid = "Market Raid";
    public const string PoorArtifact = "poor artifact";
    public const string SuperChargeArmyCamp = "Supercharge Army Camp";
    public const string SuperChargeMine = "Supercharge Mine";
    public const string TimeWarp = "Time Warp";

    public const string Raffle = "raffle";
    public const string Waffle = "waffle";
    public const string Wafaffle = "wafaffle";
    public const string Ruthless = "ruthless";

    public const int MaxLengthPlayerNameInRaffleMessage = 10;
    public const string FailedRaffleMessage = "RAFFLE OVER: Nobody entered the ";
}
