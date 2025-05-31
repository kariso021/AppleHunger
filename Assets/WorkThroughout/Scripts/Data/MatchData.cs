using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchData : MonoBehaviour
{
    private bool isWin;        // 내가 이 매치 기준으로 이겼는지 졌는지
    public string itemUnqiueId;

    public TMP_Text nameText;
    public TMP_Text ratingText;
    public TMP_Text resultText;
    public Image iconSprite = null; // 상대 아이콘 스프라이트 이미지

    public void SetMatchData(int winnerId,int rating, string playerName, string iconUniqueId)
    {
        this.itemUnqiueId = iconUniqueId;

        if (nameText != null)
            nameText.text = playerName;
        if (ratingText != null)
            ratingText.text = rating.ToString();
        if(resultText != null)
        {
            isWin = winnerId == SQLiteManager.Instance.player.playerId ? true : false;

            resultText.text = isWin ? "승리" : "패배";
            resultText.color = isWin ? new Color32(135, 236, 61, 255) : new Color32(207,73,42,255);
        }    

        
    }


}
