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
            message = "´Ğ³×ÀÓÀ» ÀÔ·ÂÇØÁÖ¼¼¿ä.";
            return false;
        }

        string allowedPattern = @"^[°¡-ÆRa-zA-Z0-9]+$";
        if (!System.Text.RegularExpressions.Regex.IsMatch(nickname, allowedPattern))
        {
            message = "´Ğ³×ÀÓÀº ÇÑ±Û, ¿µ¾î, ¼ıÀÚ¸¸ »ç¿ëÇÒ ¼ö ÀÖ¾î¿ä.";
            return false;
        }

        bool containsKorean = System.Text.RegularExpressions.Regex.IsMatch(nickname, @"[°¡-ÆR]");

        if (containsKorean)
        {
            if (nickname.Length > 7)
            {
                message = "ÇÑ±ÛÀÌ Æ÷ÇÔµÈ ´Ğ³×ÀÓÀº 7ÀÚ±îÁö »ç¿ëÇÒ ¼ö ÀÖ¾î¿ä.";
                return false;
            }
        }
        else
        {
            if (nickname.Length > 11)
            {
                message = "¿µ¾î/¼ıÀÚ ´Ğ³×ÀÓÀº 11ÀÚ±îÁö »ç¿ëÇÒ ¼ö ÀÖ¾î¿ä.";
                return false;
            }
        }

        return true;
    }


}
