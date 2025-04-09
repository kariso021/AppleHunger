using SQLite;
using System;
using System.Collections.Generic;

[Serializable]
[Table("playerItems")]
public class PlayerItemData
{
    [PrimaryKey]
    public int itemId { get; set; }
    public int playerId { get; set; }
    public int itemUniqueId { get; set; }
    public string itemType { get; set; }
    public int price { get; set; }
    public bool isUnlocked { get; set; }
    public string acquiredAt { get; set; }

    public PlayerItemData() { }

    public PlayerItemData(int itemId, int playerId, int itemUniqueId, string itemType,int price, bool isUnlocked, string acquiredAt)
    {
        this.itemId = itemId;
        this.playerId = playerId;
        this.itemUniqueId = itemUniqueId;
        this.itemType = itemType;
        this.price = price;
        this.isUnlocked = isUnlocked;
        this.acquiredAt = acquiredAt;
    }
}

[System.Serializable]
public class PlayerItemList
{
    public List<PlayerItemData> items;
}
