using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClientComboUI : MonoBehaviour
{
    [Header("Combo Text (Screen Space Canvas)")]
    [SerializeField] private TMP_Text comboText;
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = comboText.GetComponent<RectTransform>();
        canvasGroup = comboText.GetComponent<CanvasGroup>()
                      ?? comboText.gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        comboText.gameObject.SetActive(false);
    }

    public void ShowCombo(int comboCount, Vector3 worldPos)
    {
        comboText.gameObject.SetActive(true);
        comboText.text = $"Combo {comboCount}!";
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        rectTransform.anchoredPosition = screenPos -
            new Vector2(Screen.width, Screen.height) * 0.5f;

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

        comboText.gameObject.SetActive(false); // Fade 후 비활성화
    }
}
