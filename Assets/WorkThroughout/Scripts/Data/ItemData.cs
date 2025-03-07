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
    public Image itemLockedImage;
    public TMP_Text itemPriceText;
    private Button itemButton;

    private void Awake()
    {
        itemButton = GetComponent<Button>();
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

        // 기존 버튼 이벤트 제거 후 새로운 이벤트 추가
        itemButton.onClick.RemoveAllListeners();
        if (!isUnlocked)
        {
            itemButton.onClick.AddListener(() =>
            {
                Debug.Log($"🔓 아이템 구매 시도: {itemUniqueId}");
                FindAnyObjectByType<ClientNetworkManager>().PurchasePlayerItem(SQLiteManager.Instance.player.playerId, itemUniqueId);
            });
        }
        else
        {
            itemButton.onClick.AddListener(() => Debug.Log("✅ 이미 해금된 아이템"));
        }
    }

    public void UpdateItem(bool newUnlockStatus)
    {
        isUnlocked = newUnlockStatus;
        itemLockedImage.gameObject.SetActive(!isUnlocked);

        // 버튼 이벤트 갱신
        itemButton.onClick.RemoveAllListeners();
        if (isUnlocked)
        {
            itemButton.onClick.AddListener(() => Debug.Log("✅ 이미 해금된 아이템"));
        }
    }
}
