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
    /// 창을 보여주고 콜백을 설정합니다.
    /// </summary>
    /// <param name="title">창 제목</param>
    /// <param name="defaultText">기본 입력값</param>
    /// <param name="validChars">허용할 문자 집합</param>
    /// <param name="charLimit">최대 글자 수</param>
    /// <param name="onCancel">취소 시 호출될 액션</param>
    /// <param name="onSubmit">확인 시 호출될 액션(입력값)</param>
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