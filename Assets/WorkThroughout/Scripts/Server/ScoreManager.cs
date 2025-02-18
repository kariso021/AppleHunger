using FishNet.Object;
using FishNet.Connection;
using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoreManager : NetworkBehaviour
{
    private static Dictionary<NetworkConnection, int> playerScores = new Dictionary<NetworkConnection, int>();
    private static Dictionary<NetworkConnection, int> playerCombos = new Dictionary<NetworkConnection, int>();

    private int maxCombo = 5;
    private float comboTimeLimit = 5f;
    private Dictionary<NetworkConnection, Coroutine> comboCoroutines = new Dictionary<NetworkConnection, Coroutine>();

    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private int ComboValue = 1;

    public override void OnStartClient()
    {
        base.OnStartClient();

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj == null)
        {
            Debug.LogError($" {gameObject.name}의 Canvas UI를 찾을 수 없습니다!");
            return;
        }

        Transform scoreTextTransform = canvasObj.transform.Find("ScoreText");
        if (scoreTextTransform != null)
        {
            scoreText = scoreTextTransform.GetComponent<TextMeshProUGUI>();
        }

        if (scoreText == null)
        {
            Debug.LogError($"{gameObject.name}의 Score UI를 찾을 수 없습니다!");
        }

        UpdateScoreUI();
    }

    /// <summary>
    /// ✅ 서버에서 점수를 계산하고 해당 클라이언트에게 업데이트
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void AddScoreServerRpc(int appleCount, int AppleValue, NetworkConnection sender)
    {
        if (sender == null)
        {
            Debug.LogError("🚨 ServerRpc 호출자의 NetworkConnection이 null입니다!");
            return;
        }

        if (!playerScores.ContainsKey(sender))
            playerScores[sender] = 0;

        if (!playerCombos.ContainsKey(sender))
            playerCombos[sender] = 0;

        // ✅ 점수 공식 적용
        int currentCombo = playerCombos[sender];
        int TotalComboValue = ComboValue * currentCombo;
        int finalScore = (appleCount * AppleValue) + (TotalComboValue * appleCount);

        playerScores[sender] += finalScore;
        playerCombos[sender] = Mathf.Min(playerCombos[sender] + 1, maxCombo);

        UpdateScoreTargetRpc(sender, playerScores[sender], playerCombos[sender]);

        // ✅ 콤보 타이머 시작
        StartComboTimer(sender);
    }

    /// <summary>
    /// ✅ 개별 클라이언트의 UI 업데이트
    /// </summary>
    [TargetRpc]
    private void UpdateScoreTargetRpc(NetworkConnection conn, int newScore, int newCombo)
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {newScore} (Combo: {newCombo})";
        }
    }

    /// <summary>
    /// ✅ 개별 클라이언트의 콤보 타이머 실행
    /// </summary>
    private void StartComboTimer(NetworkConnection conn)
    {
        if (comboCoroutines.ContainsKey(conn) && comboCoroutines[conn] != null)
        {
            StopCoroutine(comboCoroutines[conn]);
        }
        comboCoroutines[conn] = StartCoroutine(ResetComboTimer(conn));
    }

    private IEnumerator ResetComboTimer(NetworkConnection conn)
    {
        float timer = 0f;
        while (timer <= comboTimeLimit)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (playerCombos.ContainsKey(conn))
        {
            playerCombos[conn] = 0;
        }

        UpdateScoreTargetRpc(conn, playerScores[conn], 0);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            NetworkConnection ownerConnection = base.Owner;
            int displayScore = playerScores.ContainsKey(ownerConnection) ? playerScores[ownerConnection] : 0;
            int displayCombo = playerCombos.ContainsKey(ownerConnection) ? playerCombos[ownerConnection] : 0;
            scoreText.text = $"Score: {displayScore} (Combo: {displayCombo})";
        }
    }
}
