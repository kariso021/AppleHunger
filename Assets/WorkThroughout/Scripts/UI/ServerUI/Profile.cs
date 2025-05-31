using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Profile : MonoBehaviour
{
    public Image profileImage; // 프로필 이미지
    public TMP_Text nameText;  // 유저 이름
    public TMP_Text recordText; // 전적
    public TMP_Text winText;
    public TMP_Text loseText;
    public TMP_Text ratingText; // 점수 or 랭킹

    // 프로필 데이터 설정 함수
    public void SetProfile(string name, int matches, int win, int lose, int rating)
    {
        //if (profileImage != null)
        //    profileImage.sprite = image;

        if (nameText != null)
            nameText.text = name;

        if (recordText != null)
            recordText.text = $"{matches}전";
        if (winText != null)
            winText.text = $"{win}승";
        if (loseText != null)
            loseText.text = $"{lose}패";
        if (ratingText != null)
            ratingText.text = rating.ToString();
    }

    private void Start()
    {
        DataSyncManager.Instance.OnPlayerProfileChanged += profileUIupdate;

        //profileUIupdate();
    }

    private void profileUIupdate()
    {
        if (this == null || gameObject == null)
        {
            Debug.LogWarning("[Profile] 오브젝트가 이미 파괴되었습니다.");
            return;
        }

        if (SQLiteManager.Instance.player == null || SQLiteManager.Instance.stats == null)
        {
            Debug.LogWarning("[Profile] 불완전한 데이터 상태");
            return;
        }
        Debug.Log($"[과연] 이름은 {gameObject.name}");
        SetProfile(
            SQLiteManager.Instance.player.playerName,
            SQLiteManager.Instance.stats.totalGames,
            SQLiteManager.Instance.stats.wins,
            SQLiteManager.Instance.stats.losses,
            SQLiteManager.Instance.player.rating
        );
    }


    private void OnDestroy()
    {
        if (DataSyncManager.Instance != null)
            DataSyncManager.Instance.OnPlayerProfileChanged -= profileUIupdate;
    }

}
