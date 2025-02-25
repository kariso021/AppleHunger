using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerItemData
{
    public int itemId;       // ������ ID
    public int playerId;     // �÷��̾� ID
    public string itemType;  // ������ ���� (icon, board, currency)
    public int itemValue;    // ������ ���� �Ǵ� ��ġ
    public string acquiredAt; // ȹ�� ��¥ (JSON ��ȯ�� ���� string)

    public PlayerItemData() { }
    public PlayerItemData(int itemId, int playerId, string itemType, int itemValue, string acquiredAt)
    {
        this.itemId = itemId;
        this.playerId = playerId;
        this.itemType = itemType;
        this.itemValue = itemValue;
        this.acquiredAt = acquiredAt;
    }
}

[Serializable]
public class PlayerItemsList
{
    public List<PlayerItemData> items;

    public static PlayerItemsList FromJson(string jsonData)
    {
        return JsonUtility.FromJson<PlayerItemsList>(jsonData);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}
