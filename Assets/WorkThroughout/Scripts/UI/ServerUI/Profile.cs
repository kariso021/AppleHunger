using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Profile : MonoBehaviour
{
    public Image profileImage; // 프로필 이미지
    public TMP_Text nameText;  // 유저 이름
    public TMP_Text recordText; // 전적
    public TMP_Text ratingText; // 점수 or 랭킹

    // 프로필 데이터 설정 함수
    public void SetProfile(string name,int matches,int win, int rating)
    {
        //if (profileImage != null)
        //    profileImage.sprite = image;

        if (nameText != null)
            nameText.text = name;

        if (recordText != null)
            recordText.text = $"{matches}전\n{win}승";

        if (ratingText != null)
            ratingText.text = rating.ToString();
    }

    private void Start()
    {
        DataSyncManager.Instance.OnPlayerProfileChanged += profileUIupdate;

        profileUIupdate();
    }

    private void profileUIupdate()
    {
        SetProfile(
            SQLiteManager.Instance.player.playerName,
            SQLiteManager.Instance.stats.totalGames,
            SQLiteManager.Instance.stats.wins,
            SQLiteManager.Instance.player.rating
            );
    }
}
