using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        StartCoroutine(SubscribeAfterFrame());
    }

    private void OnDisable()
    {
        DataSyncManager.Instance.OnPlayerItemsChanged -= UpdateItemList;
    }
    private IEnumerator SubscribeAfterFrame()
    {
        yield return null; // 한 프레임 기다림
        if (DataSyncManager.Instance != null)
        {
            DataSyncManager.Instance.OnPlayerItemsChanged += UpdateItemList;
        }
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

        // 20250619 기존에 GridLayout이던걸 ㅜ전ㅣ 무
        //GridLayoutGroup grid = holder.GetComponent<GridLayoutGroup>();
        //RectTransform rect = holder.GetComponent<RectTransform>();
        //AutoAdjustGridByResolution(grid, rect, grid.constraintCount);


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
        // 🔹 부모 뷰포트 또는 스크롤뷰의 width 기준
        float parentWidth = ((RectTransform)content.parent).rect.width;

        // 🔹 spacing 설정
        float spacing = 20f; // 원하는 여백 값
        grid.spacing = new Vector2(spacing, spacing);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        // 🔹 셀 크기 계산 (spacing 고려해서 너비 3등분)
        float totalSpacing = spacing * (columns - 1);
        float cellWidth = (parentWidth - totalSpacing) / columns;

        grid.cellSize = new Vector2(cellWidth, cellWidth); // 정사각형 셀

        // 🔹 총 아이템 수와 행 계산
        int totalItems = content.childCount;
        int rows = Mathf.CeilToInt((float)totalItems / columns);

        // 🔹 콘텐츠 높이 계산
        float totalHeight = rows * cellWidth + (rows - 1) * spacing;

        // 🔹 Pivot / Anchor 고정
        content.pivot = new Vector2(0f, 1f);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f); // 부모 너비 따라감

        // 🔹 콘텐츠 사이즈 설정
        content.sizeDelta = new Vector2(0, totalHeight); // width는 자동, height만 설정

        Debug.Log($"📱 최적 해상도 적용됨: parentWidth={parentWidth}, cell={cellWidth}, spacing={spacing}, rows={rows}, height={totalHeight}");
    }





}
