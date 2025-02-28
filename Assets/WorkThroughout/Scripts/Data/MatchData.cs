using TMPro;
using UnityEngine;

public class MatchData : MonoBehaviour
{
    private bool isWin;        // 내가 이 매치 기준으로 이겼는지 졌는지

    public TMP_Text nameText;
    public TMP_Text ratingText;
    public TMP_Text resultText;
    public Sprite iconSprite = null; // 상대 아이콘 스프라이트 이미지

    public void SetMatchData(int winnerId,int rating, string playerName, string iconUniqueId)
    {
        if (nameText != null)
            nameText.text = playerName;
        if (ratingText != null)
            ratingText.text = rating.ToString();
        if(resultText != null)
        {
            isWin = winnerId == SQLiteManager.Instance.player.playerId ? true : false;

            resultText.text = isWin ? "WIN" : "LOSE";
        }    
    }


}
