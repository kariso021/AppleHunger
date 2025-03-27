using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NicknameManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text resultText;

    private void Start()
    {
        confirmButton.onClick.AddListener(() => OnClick_ChangeNickname());
    }

    public void OnClick_ChangeNickname()
    {
        string newNickname = nicknameInputField.text.Trim();

        if (!IsValidNickname(newNickname))
        {
            resultText.text = "�г����� �ѱ� 1~7�� �Ǵ� ���� 1~11�ڸ� �����ؿ�.";
            Debug.Log("�����ʰ�");
            return;
        }

        int playerId = SQLiteManager.Instance.player.playerId;
        StartCoroutine(ServerToAPIManager.Instance.UpdateNicknameOnServer(newNickname));
    }

    private bool IsValidNickname(string nickname)
    {
        if (string.IsNullOrEmpty(nickname)) return false;

        // �ѱ۸�, �ִ� 7�� or ����/����, �ִ� 11�� ���
        string koreanPattern = @"^[��-�R]{1,7}$";
        string englishPattern = @"^[a-zA-Z0-9]{1,11}$";

        return System.Text.RegularExpressions.Regex.IsMatch(nickname, koreanPattern) ||
               System.Text.RegularExpressions.Regex.IsMatch(nickname, englishPattern);
    }

}
