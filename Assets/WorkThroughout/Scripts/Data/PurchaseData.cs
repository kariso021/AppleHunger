using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PurchaseData
{
    public int purchaseId;   // ���� ID
    public int playerId;     // �÷��̾� ID
    public string itemType;  // ������ ������ ���� (icon, board, currency)
    public int cost;         // ���� ���
    public string purchasedAt; // ���� ��¥ (JSON ��ȯ�� ���� string)

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
