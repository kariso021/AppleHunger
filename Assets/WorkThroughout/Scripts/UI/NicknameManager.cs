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
            resultText.text = "닉네임은 한글 1~7자 또는 영어 1~11자만 가능해요.";
            Debug.Log("범위초과");
            return;
        }

        StartCoroutine(ServerToAPIManager.Instance.UpdateNicknameOnServer(newNickname));
    }

    private bool IsValidNickname(string nickname)
    {
        if (string.IsNullOrEmpty(nickname)) return false;

        // 한글만, 최대 7자 or 영문/숫자, 최대 11자 허용
        string koreanPattern = @"^[가-힣]{1,7}$";
        string englishPattern = @"^[a-zA-Z0-9]{1,11}$";

        return System.Text.RegularExpressions.Regex.IsMatch(nickname, koreanPattern) ||
               System.Text.RegularExpressions.Regex.IsMatch(nickname, englishPattern);
    }

}
