using System;

[Serializable]
public class PlayerAddResponse
{
    public bool success; // 요청 성공 여부
    public int playerId; // MySQL에서 자동 생성된 playerId
    public PlayerData playerData;

    public PlayerAddResponse() { }

    public PlayerAddResponse(bool success, int playerId,PlayerData playerData)
    {
        this.success = success;
        this.playerId = playerId;
        this.playerData = playerData;
    }
}
