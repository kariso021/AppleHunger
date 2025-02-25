using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerItemData
{
    public int itemId;       // 아이템 ID
    public int playerId;     // 플레이어 ID
    public string itemType;  // 아이템 유형 (icon, board, currency)
    public int itemValue;    // 아이템 개수 또는 가치
    public string acquiredAt; // 획득 날짜 (JSON 변환을 위해 string)

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
