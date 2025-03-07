using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingData : MonoBehaviour
{
    public int playerId { get; private set; }
    public int rankPosition { get; private set; } // 🔹 정렬을 위한 랭크 포지션 추가
    public Sprite profileIcon;
    public TMP_Text nameText;
    public TMP_Text ratingText;
    public TMP_Text rankText;

    private void Start()
    {
        if (gameObject.tag == "Profile" || GetComponent<Button>() == null) return;

        GetComponent<Button>().onClick.AddListener(() =>
        PopupManager.Instance.ShowPopup(
            FindAnyObjectByType<RankingRecordsManager>().rankProfilePopupGameObject, playerId));
    }

    public void SetRankingData(int playerId, string playerName, int rating, int rankPosition, string profileIcon)
    {
        this.playerId = playerId;
        this.rankPosition = rankPosition; // 🔹 랭킹 포지션 저장

        if (nameText != null)
            nameText.text = playerName;
        if (ratingText != null)
            ratingText.text = rating.ToString();
        if (rankText != null)
            rankText.text = rankPosition.ToString();
    }
}
