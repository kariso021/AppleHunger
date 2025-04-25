using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class NicknameManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text placeHolderText;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private Button closeButton;

    private HashSet<string> forbiddenWords;
    private bool isChangingNickname = false;
    private void Awake()
    {
        closeButton.onClick.AddListener(() =>
        PopupManager.Instance.ClosePopup());
    }
    private void Start()
    {
        confirmButton.onClick.AddListener(OnClick_ChangeNickname);
        placeHolderText.text = SQLiteManager.Instance.player.playerName;
        forbiddenWords = ForbiddenWordsLoader.LoadForbiddenWords();
    }



    public void OnClick_ChangeNickname()
    {
        if (isChangingNickname) return;
        StartCoroutine(ChangeNicknameCoroutine());
    }

    private IEnumerator ChangeNicknameCoroutine()
    {
        string newNickname = nicknameInputField.text.Trim();

        if (!IsValidNickname(newNickname, out string message))
        {
            warningText.text = message;
            yield break;
        }

        isChangingNickname = true;
        confirmButton.interactable = false;

        // 닉네임 중복 확인
        bool isDuplicate = false;
        yield return StartCoroutine(ServerToAPIManager.Instance.CheckNicknameDuplicate(newNickname, (result) => {
            isDuplicate = result;
        }));

        if (isDuplicate)
        {
            warningText.text = "이미 사용 중인 닉네임입니다.";
            isChangingNickname = false;
            confirmButton.interactable = true;
            yield break;
        }

        // 닉네임 변경 요청
        PopupManager.Instance.ShowPopup(PopupManager.Instance.loadingPopup);
        yield return StartCoroutine(ServerToAPIManager.Instance.UpdateNicknameOnServer(newNickname));
        yield return new WaitForSeconds(0.5f);
        PopupManager.Instance.ClosePopup();

        isChangingNickname = false;
        confirmButton.interactable = true;
    }

    private bool IsValidNickname(string nickname, out string message)
    {
        message = "";

        if (string.IsNullOrWhiteSpace(nickname))
        {
            message = "닉네임을 입력해주세요.";
            return false;
        }

        //  한글, 영어, 숫자만 허용
        string allowedPattern = @"^[가-힣a-zA-Z0-9]+$";
        if (!Regex.IsMatch(nickname, allowedPattern))
        {
            message = "닉네임은 한글, 영어, 숫자만 사용할 수 있어요.";
            return false;
        }

        //  최소 1자, 최대 7자
        if (nickname.Length < 1 || nickname.Length > 7)
        {
            message = "닉네임은 1자 이상 7자 이하로 입력해주세요.";
            return false;
        }

        // 금칙어 포함 확인 (소문자로 비교)
        string lowerNickname = nickname.ToLower();
        foreach (string word in forbiddenWords)
        {
            if (lowerNickname.Contains(word))
            {
                message = "부적절한 단어가 포함되어 있습니다.";
                return false;
            }
        }

        return true;
    }



}
