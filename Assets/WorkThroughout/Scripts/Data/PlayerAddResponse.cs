using System;

[Serializable]
public class PlayerAddResponse
{
    public bool success; // ��û ���� ����
    public int playerId; // MySQL���� �ڵ� ������ playerId
    public PlayerData playerData;

    public PlayerAddResponse() { }

    public PlayerAddResponse(bool success, int playerId,PlayerData playerData)
    {
        this.success = success;
        this.playerId = playerId;
        this.playerData = playerData;
    }
}
