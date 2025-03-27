using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField nicknameInputField;  // 인풋 필드 연결
    public Button confirmButton;               // 버튼
    public TMP_Text resultText;                // 결과 출력 텍스트

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmNickname); // 버튼에 이벤트 연결
    }

    private void OnConfirmNickname()
    {
        string newNickname = nicknameInputField.text.Trim();  // 공백 제거

        if (string.IsNullOrEmpty(newNickname))
        {
            resultText.text = "닉네임을 입력해주세요!";
            return;
        }

        // 닉네임 변경 처리 (서버에 보내기 또는 로컬 저장)
        Debug.Log($"닉네임 변경: {newNickname}");
        resultText.text = $"닉네임이 '{newNickname}'(으)로 변경되었습니다!";

        // TODO: API 서버에 변경 요청 보내기 (예: ServerToAPIManager.Instance.UpdateNickname(newNickname))
    }
}
