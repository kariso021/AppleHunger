using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public GameObject itemDataIconListHolder;
    public GameObject itemDataBoardListHolder;
    private Dictionary<int, ItemData> activeItems = new Dictionary<int, ItemData>(); // 아이템 ID별 저장

    public GameObject currentItemIcon;
    public GameObject currentItemBoard;


    private void OnEnable()
    {
        DataSyncManager.Instance.OnPlayerItemsChanged += UpdateItemList;
    }

    private void OnDisable()
    {
        DataSyncManager.Instance.OnPlayerItemsChanged -= UpdateItemList;
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
        GridLayoutGroup grid = holder.GetComponent<GridLayoutGroup>();
        RectTransform rect = holder.GetComponent<RectTransform>();
        AutoAdjustGridByResolution(grid, rect, grid.constraintCount);


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

    void AutoAdjustGridByResolution(GridLayoutGroup grid, RectTransform content, int columns)
    {
        // 기준 해상도: 1080x1920 에서 Cell 300, Spacing 0
        //             1440x2560 에서 Cell 400, Spacing -100

        float referenceWidth = 1080f;
        float referenceHeight = 1920f;

        float currentWidth = Screen.width;
        float currentHeight = Screen.height;

        // 해상도 비율 계산
        float widthRatio = currentWidth / referenceWidth;
        float heightRatio = currentHeight / referenceHeight;
        float resolutionScale = (widthRatio + heightRatio) / 2f;

        // 🔹 Cell 사이즈 조정
        float baseCellSize = 300f; // 1080x1920 기준
        float cellSize = baseCellSize * resolutionScale;

        // 🔹 Spacing 계산
        float baseSpacing = 0f;
        float spacing = -100f * (resolutionScale - 1f); // 해상도가 커질수록 spacing 음수

        // 🔹 적용
        grid.cellSize = new Vector2(cellSize, cellSize);
        grid.spacing = new Vector2(spacing, spacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        // 🔹 Pivot / Anchor 고정
        content.pivot = new Vector2(0f, 1f);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(0f, 1f);

        // 🔹 Content 너비 설정
        float totalWidth = columns * cellSize + (columns - 1) * spacing;
        content.sizeDelta = new Vector2(totalWidth, content.sizeDelta.y);

        Debug.Log($"📱 해상도 자동 적용됨: {currentWidth}x{currentHeight} → 셀 {cellSize}, spacing {spacing}");
    }



}
