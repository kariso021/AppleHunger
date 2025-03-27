using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nicknameInputField;  // ��ǲ �ʵ� ����
    public Button confirmButton;               // ��ư
    public TMP_Text resultText;                // ��� ��� �ؽ�Ʈ

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmNickname); // ��ư�� �̺�Ʈ ����
    }

    private void OnConfirmNickname()
    {
        string newNickname = nicknameInputField.text.Trim();  // ���� ����

        if (string.IsNullOrEmpty(newNickname))
        {
            resultText.text = "�г����� �Է����ּ���!";
            return;
        }

        // �г��� ���� ó�� (������ ������ �Ǵ� ���� ����)
        Debug.Log($"�г��� ����: {newNickname}");
        resultText.text = $"�г����� '{newNickname}'(��)�� ����Ǿ����ϴ�!";

        // TODO: API ������ ���� ��û ������ (��: ServerToAPIManager.Instance.UpdateNickname(newNickname))
    }
}
