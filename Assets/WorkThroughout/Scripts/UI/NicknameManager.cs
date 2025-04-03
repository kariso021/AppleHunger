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

    private bool isChangingNickname = false;
    private void Start()
    {
        confirmButton.onClick.AddListener(() => OnClick_ChangeNickname());
    }

    public void OnClick_ChangeNickname()
    {
        string newNickname = nicknameInputField.text.Trim();

        if (!IsValidNickname(newNickname,out string message))
        {
            resultText.text = message;
            return;
        }

        StartCoroutine(ServerToAPIManager.Instance.UpdateNicknameOnServer(newNickname));
    }

    private bool IsValidNickname(string nickname, out string message)
    {
        message = "";

        if (string.IsNullOrEmpty(nickname))
        {
            message = "�г����� �Է����ּ���.";
            return false;
        }

        string allowedPattern = @"^[��-�Ra-zA-Z0-9]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(nickname, allowedPattern))
        {
            message = "�г����� �ѱ�, ����, ���ڸ� ����� �� �־��.";
            return false;
        }

        bool containsKorean = System.Text.RegularExpressions.Regex.IsMatch(nickname, @"[��-�R]");

        if (containsKorean)
        {
            if (nickname.Length > 7)
            {
                message = "�ѱ��� ���Ե� �г����� 7�ڱ��� ����� �� �־��.";
                return false;
            }
        }
        else
        {
            if (nickname.Length > 11)
            {
                message = "����/���� �г����� 11�ڱ��� ����� �� �־��.";
                return false;
            }
        }

        return true;
    }


}
