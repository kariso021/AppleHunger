using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PurchaseData
{
    public int purchaseId;   // 구매 ID
    public int playerId;     // 플레이어 ID
    public string itemType;  // 구매한 아이템 유형 (icon, board, currency)
    public int cost;         // 구매 비용
    public string purchasedAt; // 구매 날짜 (JSON 변환을 위해 string)

    public static PurchaseData FromJson(string jsonData)
    {
        return JsonUtility.FromJson<PurchaseData>(jsonData);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}

[Serializable]
public class PurchaseList
{
    public List<PurchaseData> purchases;

    public static PurchaseList FromJson(string jsonData)
    {
        return JsonUtility.FromJson<PurchaseList>(jsonData);
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }
}
