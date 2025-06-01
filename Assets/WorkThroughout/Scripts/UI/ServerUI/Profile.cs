using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Profile : MonoBehaviour
{
    public Image profileImage; // ������ �̹���
    public TMP_Text nameText;  // ���� �̸�
    public TMP_Text recordText; // ����
    public TMP_Text winText;
    public TMP_Text loseText;
    public TMP_Text ratingText; // ���� or ��ŷ

    // ������ ������ ���� �Լ�
    public void SetProfile(string name, int matches, int win, int lose, int rating)
    {
        //if (profileImage != null)
        //    profileImage.sprite = image;

        if (nameText != null)
            nameText.text = name;

        if (recordText != null)
            recordText.text = $"{matches}��";
        if (winText != null)
            winText.text = $"{win}��";
        if (loseText != null)
            loseText.text = $"{lose}��";
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
            Debug.LogWarning("[Profile] ������Ʈ�� �̹� �ı��Ǿ����ϴ�.");
            return;
        }

        if (SQLiteManager.Instance.player == null || SQLiteManager.Instance.stats == null)
        {
            Debug.LogWarning("[Profile] �ҿ����� ������ ����");
            return;
        }
        Debug.Log($"[����] �̸��� {gameObject.name}");
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
