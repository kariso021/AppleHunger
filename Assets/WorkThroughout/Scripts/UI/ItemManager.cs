using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public GameObject itemDataIconListHolder;
    public GameObject itemDataBoardListHolder;
    private Dictionary<int, ItemData> activeItems = new Dictionary<int, ItemData>(); // 아이템 ID별 저장

    public GameObject currentItemIcon;
    public GameObject currentItemBoard;

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

        // 현재 사용중인 아이템 표시를 위함
        currentItemIcon.GetComponent<ItemData>().SetItemData(SQLiteManager.Instance.player.profileIcon);
        currentItemBoard.GetComponent<ItemData>().SetItemData(SQLiteManager.Instance.player.boardImage);
        currentItemIcon.GetComponent<ItemData>().itemIcon = currentItemIcon.GetComponent<Image>();
        currentItemBoard.GetComponent<ItemData>().itemIcon = currentItemBoard.GetComponent<Image>();

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

                if (type == "icon")
                    AddressableManager.Instance.itemIconObj.Add(itemInstance);
                else
                    AddressableManager.Instance.itemBoardObj.Add(itemInstance);
            }
            
        }

        if (type == "icon")
        {
            if(!AddressableManager.Instance.itemIconObj.Contains(currentItemIcon))
                AddressableManager.Instance.itemIconObj.Add(currentItemIcon);
            Debug.Log("=================아이콘 이미지 변경=======================");
            AddressableManager.Instance.LoadItemIconFromGroup();
        }
        else
        {
            if (!AddressableManager.Instance.itemBoardObj.Contains(currentItemBoard))
                AddressableManager.Instance.itemBoardObj.Add(currentItemBoard);
            Debug.Log("==================보드 이미지 변경==========================");
            AddressableManager.Instance.LoadItemBoardFromGroup();
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
