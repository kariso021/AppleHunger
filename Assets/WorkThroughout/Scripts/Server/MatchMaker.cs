using System.Collections.Generic;

public class Matchmaker
{
    private Queue<int> waitingPlayers = new Queue<int>();

    public void AddPlayerToQueue(int playerId)
    {
        waitingPlayers.Enqueue(playerId);
        TryMatchPlayers();
    }

    private void TryMatchPlayers()
    {
        if (waitingPlayers.Count >= 2)
        {
            int player1 = waitingPlayers.Dequeue();
            int player2 = waitingPlayers.Dequeue();

        }
    }
}
