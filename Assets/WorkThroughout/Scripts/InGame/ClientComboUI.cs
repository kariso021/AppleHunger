using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientComboUI : MonoBehaviour
{
    public static ClientComboUI Instance { get; private set; }

    [Header("Combo Text (Screen Space Canvas)")]
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        rectTransform = comboText.GetComponent<RectTransform>();
        canvasGroup = comboText.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = comboText.gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// worldPos ��ġ ���� �޺� �ؽ�Ʈ�� ���� ���̵� �ƿ��մϴ�.
    /// </summary>
    public void ShowCombo(int comboCount, Vector3 worldPos)
    {
        // 1) �ؽ�Ʈ ����
        comboText.text = $"Combo {comboCount}!";

        // 2) ����潺ũ�� ��ǥ ��ȯ �� UI ��ġ ����
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        // Canvas�� Screen Space - Overlay ���:
        rectTransform.anchoredPosition = screenPos - new Vector2(Screen.width, Screen.height) * 0.5f;

        // 3) ���̵� ����Ʈ
        StopAllCoroutines();
        StartCoroutine(FadeCoroutine());
    }

    private IEnumerator FadeCoroutine()
    {
        float elapsed = 0f;
        canvasGroup.alpha = 1f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }
}
