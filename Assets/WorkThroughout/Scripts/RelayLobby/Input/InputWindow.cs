using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InputWindow : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button okButton;
    [SerializeField] private Button cancelButton;

    private Action<string> onSubmit;
    private Action onCancel;
    private string validCharacters;

    private void Awake()
    {
        gameObject.SetActive(false);

        okButton.onClick.AddListener(() =>
        {
            Hide();
            onSubmit?.Invoke(inputField.text);
        });

        cancelButton.onClick.AddListener(() =>
        {
            Hide();
            onCancel?.Invoke();
        });
    }

    private void Update()
    {
        if (gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                okButton.onClick.Invoke();
            else if (Input.GetKeyDown(KeyCode.Escape))
                cancelButton.onClick.Invoke();
        }
    }

    /// <summary>
    /// â�� �����ְ� �ݹ��� �����մϴ�.
    /// </summary>
    /// <param name="title">â ����</param>
    /// <param name="defaultText">�⺻ �Է°�</param>
    /// <param name="validChars">����� ���� ����</param>
    /// <param name="charLimit">�ִ� ���� ��</param>
    /// <param name="onCancel">��� �� ȣ��� �׼�</param>
    /// <param name="onSubmit">Ȯ�� �� ȣ��� �׼�(�Է°�)</param>
    public void Show(
        string title,
        string defaultText,
        string validChars,
        int charLimit,
        Action onCancel,
        Action<string> onSubmit
    )
    {
        this.onCancel = onCancel;
        this.onSubmit = onSubmit;
        this.validCharacters = validChars;

        titleText.text = title;
        inputField.text = defaultText;
        inputField.characterLimit = charLimit;
        inputField.onValidateInput = ValidateChar;

        gameObject.SetActive(true);
        inputField.Select();
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private char ValidateChar(string text, int charIndex, char addedChar)
    {
        return validCharacters.IndexOf(addedChar) != -1 ? addedChar : '\0';
    }
}