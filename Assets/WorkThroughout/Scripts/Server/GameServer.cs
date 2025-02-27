using FishNet.Object;
using UnityEngine;

public class GameServer : NetworkBehaviour
{
    public static GameServer Instance;

    public Matchmaker Matchmaker;
    public RoomManager RoomManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Matchmaker = new Matchmaker();
        RoomManager = new RoomManager();
    }
}
