using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class MatchHistory : MonoBehaviour
{
    public Image profileImage; // ������ �̹���
    public TMP_Text nameText;  // ���� �̸�
    public TMP_Text resultText; // ����
    public TMP_Text ratingText; // ���� or ��ŷ

    // ������ ������ ���� �Լ�
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
