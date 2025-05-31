using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchData : MonoBehaviour
{
    private bool isWin;        // ���� �� ��ġ �������� �̰���� ������
    public string itemUnqiueId;

    public TMP_Text nameText;
    public TMP_Text ratingText;
    public TMP_Text resultText;
    public Image iconSprite = null; // ��� ������ ��������Ʈ �̹���

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

            resultText.text = isWin ? "�¸�" : "�й�";
            resultText.color = isWin ? new Color32(135, 236, 61, 255) : new Color32(207,73,42,255);
        }    

        
    }


}
