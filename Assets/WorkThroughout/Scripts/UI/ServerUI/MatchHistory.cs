using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MatchHistory : MonoBehaviour
{
    public Image profileImage; // 프로필 이미지
    public TMP_Text nameText;  // 유저 이름
    public TMP_Text resultText; // 전적
    public TMP_Text ratingText; // 점수 or 랭킹

    // 프로필 데이터 설정 함수
    public void SetProfile(Sprite image, string name, int rating, bool isWin)
    {
        if (profileImage != null)
            profileImage.sprite = image;

        if (nameText != null)
            nameText.text = name;

        if (resultText != null)
            resultText.text = isWin ? "WIN" : "LOSE";

        if (ratingText != null)
            ratingText.text = "Rating: " + rating.ToString();
    }
}
