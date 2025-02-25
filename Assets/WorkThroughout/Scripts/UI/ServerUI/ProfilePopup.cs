using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ProfilePopup : MonoBehaviour
{
    // �⺻ ����
    public Image profileImage; // ������ �̹���
    public TMP_Text nameText;  // ���� �̸�
    public TMP_Text ratingText; // ���� or ��ŷ

    // �߰� ���� ����
    public TMP_Text matchesText;
    public TMP_Text winText;
    public TMP_Text loseText;
    public TMP_Text winrateText;
    public TMP_Text collectionIconText;
    public TMP_Text collectionBoardText;
    
    /// <summary>
    /// �����ʿ� ���� ���� ������ �����ִ� �˾� ������ �����ϴ� �Լ�.
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
    public void SetProfile(Sprite image, string name, int matches, int win, int lose, int rating,
                       int collectionIcon, int collectionBoard)
    {
        Debug.Log("���� ���� ����");
        // �⺻ ���� ����
        if (profileImage != null)
            profileImage.sprite = image;

        if (nameText != null)
            nameText.text = name;

        if (matchesText != null)
            matchesText.text = matches.ToString();

        if (winText != null)
            winText.text = win.ToString();

        if (loseText != null)
            loseText.text = lose.ToString();

        if (ratingText != null)
            ratingText.text = $"{rating}";

        // �·� ��� �� ����
        float winRate = matches > 0 ? (float)win / matches * 100f : 0f;
        if (winrateText != null)
            winrateText.text = $"{winRate:F2}%";

        // �÷��� ���� ���� (������ ������ & ����)
        if (collectionIconText != null)
            collectionIconText.text = $"{collectionIcon}";

        if (collectionBoardText != null)
            collectionBoardText.text = $"{collectionBoard}";

        Debug.Log("���� ���� ��");
    }
    //private void UpdateMatchHistory(List<MatchHistory> matchHistoryData)
    //{
    //    if (matchHistory == null) return;

    //    matchHistory.Clear(); // ���� ��� ����

    //    foreach (MatchHistory match in matchHistoryData)
    //    {
    //        matchHistory.Add(match);
    //    }
    //}

}
