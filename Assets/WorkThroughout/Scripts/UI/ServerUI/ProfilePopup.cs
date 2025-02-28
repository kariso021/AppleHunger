using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ProfilePopup : MonoBehaviour
{
    // 기본 정보
    public Image profileImage; // 프로필 이미지
    public TMP_Text nameText;  // 유저 이름
    public TMP_Text ratingText; // 점수 or 랭킹

    // 추가 세부 정보
    public TMP_Text matchesText;
    public TMP_Text winText;
    public TMP_Text loseText;
    public TMP_Text winrateText;
    public TMP_Text collectionIconText;
    public TMP_Text collectionBoardText;

    /// <summary>
    /// 프로필에 대한 세부 정보를 보여주는 팝업 정보를 설정하는 함수.
    /// </summary>
    /// <param name="image"></param>
    /// <param name="name"></param>
    /// <param name="matches"></param>
    /// <param name="win"></param>
    /// <param name="loss"></param>
    /// <param name="rating"></param>
    /// <param name="collectionIcon"></param>
    /// <param name="collectionBoard"></param>
    /// <param name="matchHistoryData"></param>
    public void SetProfile(string name, int matches, int win, int lose, float winRate, int rating,
                       int collectionIcon, int collectionBoard)
    {
        Debug.Log("정보 설정 시작");
        // 기본 정보 설정
        //if (profileImage != null)
        //    profileImage.sprite = image;

        if (nameText != null)
            nameText.text = name;

        if (matchesText != null)
            matchesText.text = $"{matches}";

        if (winText != null)
            winText.text = $"{win}";

        if (loseText != null)
            loseText.text = $"{lose}";

        if (winrateText != null)
            winrateText.text = $"{winRate:F2}%";

        if (ratingText != null)
            ratingText.text = $"{rating}";

        // 컬렉션 수
        if (collectionIconText != null)
            collectionIconText.text = $"{collectionIcon}";

        if (collectionBoardText != null)
            collectionBoardText.text = $"{collectionBoard}";

        Debug.Log("정보 설정 끝");
    }
}
