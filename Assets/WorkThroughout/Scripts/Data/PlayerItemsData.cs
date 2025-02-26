using System;
using System.Collections.Generic;

[Serializable]
public class PlayerItemData
{
    public int itemId;       // ������ �������� ���� ID
    public int playerId;     // �÷��̾� ID
    public int itemUniqueId; // ���� �� �������� ���� ID , icon�� 101~ , board�� 201~ �� ����
    public string itemType;  // ������ ���� ("icon", "board")
    public bool isUnlocked;  // ������ �ر� ����
    public string acquiredAt; // ������ ȹ�� ��¥ (JSON ��ȯ�� ���� ���ڿ�)

    public PlayerItemData() { }

    public PlayerItemData(int itemId, int playerId, int itemUniqueId, string itemType, bool isUnlocked, string acquiredAt)
    {
        this.itemId = itemId;
        this.playerId = playerId;
        this.itemUniqueId = itemUniqueId;
        this.itemType = itemType;
        this.isUnlocked = isUnlocked;
        this.acquiredAt = acquiredAt;
    }
}

[System.Serializable]
public class PlayerItemList
{
    public List<PlayerItemData> items;
}
