[System.Serializable]
public class PlayerDetails
{
    public int playerId;
    public string playerName;
    public int rating;
    public string profileIcon;
    public int totalGames;
    public int wins;
    public int losses;
    public float winRate;
    public int unlockIcons;
    public int unlockBoards;

    public override string ToString()
    {
        return $"PlayerDetails => " +
               $"ID: {playerId}, Name: {playerName}, Rating: {rating}, " +
               $"ProfileIcon: {profileIcon}, TotalGames: {totalGames}, " +
               $"Wins: {wins}, Losses: {losses}, WinRate: {winRate}, " +
               $"UnlockedIcons: {unlockIcons}, UnlockedBoards: {unlockBoards}";
    }
}

[System.Serializable]
public class PlayerDetailsResponse
{
    public bool success;
    public PlayerDetails playerDetails;
}
