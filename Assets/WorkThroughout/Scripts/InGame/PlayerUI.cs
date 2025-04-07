using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.Netcode;

public class PlayerUI : MonoBehaviour
{
    [Header("Score UI")]
    [SerializeField] private TextMeshProUGUI myScoreText;
    [SerializeField] private TextMeshProUGUI opponentScoreText;

    [Header("Image UI")]
    [SerializeField] private Image Myprofileimage;
    [SerializeField] private Image Opponentprofileimage;

    [Header("Timer UI")]
    [SerializeField] private Slider timerSlider;

    [Header("NickName UI")]
    [SerializeField] private TextMeshProUGUI myNicknameText;
    [SerializeField] private TextMeshProUGUI opponentNicknameText;

    [Header("Rating UI")]
    [SerializeField] private TextMeshProUGUI myRatingText;
    [SerializeField] private TextMeshProUGUI opponentRatingText;

//  타이머 슬라
    private Dictionary<ulong, int> playerScores = new Dictionary<ulong, int>();
    private ulong myClientId;

    public static PlayerUI Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerInitialized += SetPlayerId;
        GameTimer.OnTimerUpdated += UpdateTimerUI; // 타이머 업데이트 이벤트 연결
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerInitialized -= SetPlayerId;
        GameTimer.OnTimerUpdated -= UpdateTimerUI; // 타이머 이벤트 해제
    }

    ///  `PlayerController`에서 받은 ID를 설정
    private void SetPlayerId(ulong clientId)
    {
        myClientId = NetworkManager.Singleton.LocalClientId;
        //유저 이미지 프로필 업로드
        UploadProfileImageSelf();
        UploadNickNameSelf();
        UploadRatingSelf();
        //상대방 이미지 프로필 업로드
    }

    /// 서버에서 전달된 점수를 받아 UI 업데이트 (ClientRpc로 호출됨)
    public void UpdateScoreUI(ulong clientId, int newScore)
    {
        if (Instance != null)
        {
            Instance.SetScoreUI(clientId, newScore);
        }
    }

    /// 개별 플레이어 UI 업데이트
    private void SetScoreUI(ulong playerId, int newScore)
    {
        //playerID 가 아니라 Client ID임 현재
        Debug.Log($"[Client] SetScoreUI - PlayerID: {playerId}, New Score: {newScore}, MyClientID: {myClientId}");

        playerScores[playerId] = newScore;
        RefreshUI();
    }

    // UI 갱신 (내 점수와 상대 점수 구분)
    private void RefreshUI()
    {
        if (playerScores.ContainsKey(myClientId))
        {
            myScoreText.text = $"My Score: {playerScores[myClientId]}";
        }

        foreach (var kvp in playerScores)
        {
            if (kvp.Key != myClientId)
            {
                opponentScoreText.text = $"Opponent Score: {kvp.Value}";
                break; // 상대방 점수 하나만 표시 (멀티플레이어일 경우 리스트화 가능)
            }
        }
    }

    // 타이머 UI 업데이트
    private void UpdateTimerUI(float remainingTime)
    {
        if (timerSlider != null)
        {
            timerSlider.value = remainingTime / 60f; // 60초 기준으로 슬라이더 값 조정
        }
    }

    //---------------------------Self 프로필 이미지, 닉네임, 레이팅  동기화
    private void UploadProfileImageSelf()
    {
        string ImagePath = SQLiteManager.Instance.player.profileIcon;
        AddressableManager.Instance.LoadImageFromGroup(ImagePath, Myprofileimage);
    }

    private void UploadRatingSelf()
    {
        int rating = SQLiteManager.Instance.player.rating;
        myRatingText.text = $"R :{rating}";
    }

    private void UploadNickNameSelf()
    {
        string nickname = SQLiteManager.Instance.player.playerName;
        myNicknameText.text = $"{nickname}";
    }




    //--------------------------------------------------------------------------------------

   



    public void SetOpponentIconImage(string iconKey)
    {
        Debug.Log($"[PlayerUI] 상대방 아이콘 로딩: {iconKey}");
        AddressableManager.Instance.LoadImageFromGroup(iconKey, Opponentprofileimage);
    }

    public void SetOpponentNickName(string nickname)
    {
        opponentNicknameText.text = nickname;
    }

    public void SetOpponentRating(int rating)
    {
        opponentRatingText.text = $"R : {rating}";
    }



}
