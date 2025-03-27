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
            resultText.text = "´Ð³×ÀÓÀº ÇÑ±Û 1~7ÀÚ ¶Ç´Â ¿µ¾î 1~11ÀÚ¸¸ °¡´ÉÇØ¿ä.";
            Debug.Log("¹üÀ§ÃÊ°ú");
            return;
        }

        int playerId = SQLiteManager.Instance.player.playerId;
        StartCoroutine(ServerToAPIManager.Instance.UpdateNicknameOnServer(newNickname));
    }

    private bool IsValidNickname(string nickname)
    {
        if (string.IsNullOrEmpty(nickname)) return false;

        // ÇÑ±Û¸¸, ÃÖ´ë 7ÀÚ or ¿µ¹®/¼ýÀÚ, ÃÖ´ë 11ÀÚ Çã¿ë
        string koreanPattern = @"^[°¡-ÆR]{1,7}$";
        string englishPattern = @"^[a-zA-Z0-9]{1,11}$";

        return System.Text.RegularExpressions.Regex.IsMatch(nickname, koreanPattern) ||
               System.Text.RegularExpressions.Regex.IsMatch(nickname, englishPattern);
    }

}
