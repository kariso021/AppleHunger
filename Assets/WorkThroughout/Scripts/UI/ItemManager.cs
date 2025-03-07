using System.Collections.Generic;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public GameObject itemDataIconListHolder;
    public GameObject itemDataBoardListHolder;
    private Dictionary<int, ItemData> activeItems = new Dictionary<int, ItemData>(); // 아이템 ID별 저장

    private void Start()
    {
        // ✅ 아이템 변경 이벤트 구독 (자동 갱신)
        DataSyncManager.Instance.OnPlayerItemsChanged += UpdateItemList;
    }

    // ✅ 아이템 리스트 생성 또는 갱신
    public void CreateItemList(string type)
    {
        List<PlayerItemData> playerItemsList = SQLiteManager.Instance.LoadPlayerItems();
        if (playerItemsList.Count == 0) return;

        GameObject holder = type == "icon" ? itemDataIconListHolder : itemDataBoardListHolder;

        foreach (var itemData in playerItemsList)
        {
            if (itemData.itemType != type) continue;

            // 🔹 이미 존재하는 경우 UI 업데이트만 수행
            if (activeItems.ContainsKey(itemData.itemUniqueId))
            {
                activeItems[itemData.itemUniqueId].SetItemData(
                    itemData.playerId,
                    itemData.itemUniqueId,
                    itemData.itemType,
                    itemData.price,
                    itemData.isUnlocked,
                    itemData.acquiredAt
                );
            }
            else
            {
                // 🔹 Object Pool에서 아이템 가져오기
                GameObject itemInstance = ObjectPoolManager.Instance.GetFromPool("ItemData", Vector3.zero, Quaternion.identity, holder.transform);
                if (itemInstance == null) continue;

                ItemData newItem = itemInstance.GetComponent<ItemData>();
                newItem.SetItemData(
                    itemData.playerId,
                    itemData.itemUniqueId,
                    itemData.itemType,
                    itemData.price,
                    itemData.isUnlocked,
                    itemData.acquiredAt
                );

                activeItems[itemData.itemUniqueId] = newItem;
            }
        }
    }

    // ✅ 데이터 변경 시 자동 갱신
    private void UpdateItemList()
    {
        Debug.Log("🔄 [ItemManager] 아이템 데이터 변경 감지 → UI 갱신");
        CreateItemList("icon");
        CreateItemList("board");
    }
}
