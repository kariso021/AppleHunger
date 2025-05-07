using System.Collections;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEngine;

public class ComboUI : MonoBehaviour
{
    private TMP_Text comboText;
    [SerializeField] private float fadeDuration = 1.0f;
    void Start()
    {
        comboText = GetComponentInChildren<TMP_Text>();    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowComboEffect()
    {
        if (comboText != null)
        {
            comboText.text = $"Combo {ScoreManagerSingle.Instance.ComboCount} !";

            StartCoroutine(nameof(FadeOutText));
        }
    }

    private IEnumerator FadeOutText()
    {
        Color originalColor = comboText.color;
        comboText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            comboText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 완전히 투명하게 설정 (혹시 마지막 프레임이 부족했을 경우 대비)
        comboText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
}
