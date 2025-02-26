using System;
using System.Collections.Generic;

[Serializable]
public class PlayerItemData
{
    public int itemId;       // 보유한 아이템의 고유 ID
    public int playerId;     // 플레이어 ID
    public int itemUniqueId; // 게임 내 아이템의 고유 ID , icon은 101~ , board는 201~ 로 시작
    public string itemType;  // 아이템 유형 ("icon", "board")
    public bool isUnlocked;  // 아이템 해금 여부
    public string acquiredAt; // 아이템 획득 날짜 (JSON 변환을 위해 문자열)

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
