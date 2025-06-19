using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemData : MonoBehaviour
{
    public int itemId;       // 보유한 아이템의 고유 ID
    public int playerId;     // 플레이어 ID
    public int itemUniqueId; // 게임 내 아이템의 고유 ID , icon은 101~ , board는 201~ 로 시작
    public string itemType;  // 아이템 유형 ("icon", "board")
    public int price;        // 아이템 가격
    public bool isUnlocked;  // 아이템 해금 여부
    public string acquiredAt; // 아이템 획득 날짜 (JSON 변환을 위해 문자열)

    public Image itemIcon;
    public GameObject itemLockedImage;
    public TMP_Text itemPriceText;
    public TMP_Text itemNameText;
    public Button itemButton;

    public bool isPurchasing = false;

    private void Awake()
    {
        itemButton = GetComponentInChildren<Button>();
        //Debug.Log($"[아이템체크] id : {itemUniqueId} , {itemButton.gameObject.name}");
    }

    public void SetItemData(int playerId, int itemUniqueId, string itemType, int price, bool isUnlocked, string acquiredAt)
    {
        this.playerId = playerId;
        this.itemUniqueId = itemUniqueId;
        this.itemType = itemType;
        this.price = price;
        this.isUnlocked = isUnlocked;
        this.acquiredAt = acquiredAt;

        // UI 업데이트
        itemPriceText.text = price.ToString();
        itemLockedImage.gameObject.SetActive(!isUnlocked);
        itemNameText.text = selectItemNameByItemId(itemUniqueId);


        itemButton.onClick.RemoveAllListeners();
        if (!isUnlocked) // 현재 아이템이 해금 상태일 경우
        {
            itemButton.onClick.AddListener(() =>
            {
                if (isPurchasing) return;       // ✅ 이미 구매 중이면 무시
                isPurchasing = true;            // ✅ 구매 시작

                Debug.Log($"[Item] Try purchase item : {itemUniqueId}");
                PopupManager.Instance.ShowLoading("구매");
                StartCoroutine(PurchaseItemCoroutine());
            });
        }
        else // 현재 아이템이 잠금 상태일 경우
        {
            itemButton.onClick.AddListener(() => applySelectItemDataToCurrentItemData(itemType));
            itemPriceText.text = "보유중";
        }
    }
    private IEnumerator PurchaseItemCoroutine()
    {
        if(SQLiteManager.Instance.player.currency < price)
        {
            PopupManager.Instance.ChangeLoadingText("구매");
            yield return new WaitForSeconds(0.5f);
            PopupManager.Instance.HideLoading();
            isPurchasing = false; // ✅ 완료 후 다시 클릭 가능
            yield break;
        }

        yield return ClientNetworkManager.Instance.PurchasePlayerItem(
           SQLiteManager.Instance.player.playerId, itemUniqueId
        );
        Debug.Log("[Item] Complete Purchasing");
        yield return new WaitForSeconds(1f);
        PopupManager.Instance.HideLoading();
        isPurchasing = false; // ✅ 완료 후 다시 클릭 가능
    }
    /// <summary>
    /// Collection 에서 현재 내가 사용하고 있는 아이콘,보드 이미지를 보여주기 위한 데이터만을 저장하는 함수
    /// 그 외 용도는 SetItemData(int playerId, int itemUniqueId, string itemType, int price, bool isUnlocked, string acquiredAt) 참조
    /// </summary>
    /// <param name="itemUniqueId"></param>
    public void SetItemData(string itemUniqueId)
    {
        this.itemUniqueId = int.Parse(itemUniqueId); // stoi 같은거
    }

    public void UpdateItem(bool newUnlockStatus)
    {
        isUnlocked = newUnlockStatus;
        itemLockedImage.gameObject.SetActive(!isUnlocked);

        // 버튼 이벤트 갱신
        itemButton.onClick.RemoveAllListeners();
        if (isUnlocked)
        {
            itemButton.onClick.AddListener(() => applySelectItemDataToCurrentItemData(itemType));
        }
    }

    private void applySelectItemDataToCurrentItemData(string itemType)
    {
        if (itemType == "icon")
        {
            FindAnyObjectByType<ItemManager>().currentItemIcon.GetComponent<Image>().sprite = itemIcon.sprite;
            FindAnyObjectByType<ItemManager>().currentItemIcon.GetComponent<ItemData>().itemUniqueId = this.itemUniqueId;
        }
        else
        {
            FindAnyObjectByType<ItemManager>().currentItemBoard.GetComponent<Image>().sprite = itemIcon.sprite;
            FindAnyObjectByType<ItemManager>().currentItemBoard.GetComponent<ItemData>().itemUniqueId = this.itemUniqueId;
        }

    }

    private string selectItemNameByItemId(int itemUniqueId)
    {
        switch (itemUniqueId % 100)
        {
            case 1: // bunny;
                return "토끼";
            case 2: // dragon
                return "용";
            case 3: // rat
                return "쥐";
            case 4: // sheep
                return "양";
            case 5: // monkey
                return "원숭이";
            case 6: // tiger
                return "호랑이";
            default:
                return "None";
        }
    }
}
