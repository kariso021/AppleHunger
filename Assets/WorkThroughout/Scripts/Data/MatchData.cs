using TMPro;
using UnityEngine;

public class MatchData : MonoBehaviour
{
    private bool isWin;        // ���� �� ��ġ �������� �̰���� ������

    public TMP_Text nameText;
    public TMP_Text ratingText;
    public TMP_Text resultText;
    public Sprite iconSprite = null; // ��� ������ ��������Ʈ �̹���

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
