using UnityEngine;
using TMPro;

public class FontChanger : MonoBehaviour
{
    public static FontChanger Instance;
    public TMP_FontAsset defaultFont;
    public TMP_FontAsset[] availableFonts;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //LoadFont();
    }

    private void OnEnable()
    {
        ChangeFont(defaultFont);
    }

    public void ChangeFont(TMP_FontAsset newFont)
    {
        PlayerPrefs.SetString("SelectedFont", newFont.name);
        PlayerPrefs.Save(); // 즉시 저장
        ApplyFont(newFont);
    }

    private void ApplyFont(TMP_FontAsset font)
    {
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            text.font = font;
        }
    }

    private void LoadFont()
    {
        string savedFontName = PlayerPrefs.GetString("SelectedFont", defaultFont.name);

        foreach (TMP_FontAsset font in availableFonts)
        {
            if (font.name == savedFontName)
            {
                ApplyFont(font);
                return;
            }
        }

        ApplyFont(defaultFont);
    }
}
