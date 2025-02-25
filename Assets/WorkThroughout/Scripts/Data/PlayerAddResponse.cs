using System;

[Serializable]
public class PlayerAddResponse
{
    public bool success; // ��û ���� ����
    public int playerId; // MySQL���� �ڵ� ������ playerId

    public PlayerAddResponse() { }

    public PlayerAddResponse(bool success, int playerId)
    {
        this.success = success;
        this.playerId = playerId;
    }
}
