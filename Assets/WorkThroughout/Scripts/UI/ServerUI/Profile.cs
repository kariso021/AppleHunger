using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Profile : MonoBehaviour
{
    public Image profileImage; // ������ �̹���
    public TMP_Text nameText;  // ���� �̸�
    public TMP_Text recordText; // ����
    public TMP_Text ratingText; // ���� or ��ŷ

    // ������ ������ ���� �Լ�
    public void SetProfile(Sprite image, string name,int matches,int win, int rating)
    {
        if (profileImage != null)
            profileImage.sprite = image;

        if (nameText != null)
            nameText.text = name;

        if (recordText != null)
            recordText.text = $"{matches}��\n{win}��";

        if (ratingText != null)
            ratingText.text = "Rating: " + rating.ToString();
    }
}
