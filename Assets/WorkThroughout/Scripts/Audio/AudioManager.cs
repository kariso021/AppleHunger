using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using appleHunger;
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [Header("Sound Clips")]
    public List<AudioClip> bgmClipList;
    public List<AudioClip> vfxClipList;
    [Header("Audio Sources")]
    public List<AudioSource> bgmSources;
    public List<AudioSource> vfxSources;

    [Header("Volume Controls")]
    [Range(0, 1)] public float bgmVolume = 1f;
    [Range(0, 1)] public float vfxVolume = 1f;

    private bool isBGMMuted = false;
    private bool isVFXMuted = false;
    private bool isInGameScene = false;
    [Header("UI Components")]
    public Slider bgmSlider;
    public Slider vfxSlider;
    public Toggle bgmToggle;
    public Toggle vfxToggle;

    private const string BGM_VOLUME_KEY = "BGM_VOLUME";
    private const string VFX_VOLUME_KEY = "VFX_VOLUME";
    private const string BGM_MUTE_KEY = "BGM_MUTED";
    private const string VFX_MUTE_KEY = "VFX_MUTED";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        else
            Destroy(gameObject);
    }

    private void Start()
    {
        addListnerInit();
        SceneManager.sceneLoaded += OnSceneLoaded;

        LoadSettings();
        ApplyVolumes();
        ApplyUI();


    }

    private void addListnerInit()
    {
        bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        vfxSlider.onValueChanged.AddListener(OnVFXVolumeChanged);
        bgmToggle.onValueChanged.AddListener(ToggleBGMMute);
        vfxToggle.onValueChanged.AddListener(ToggleVFXMute);
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 로드됨: {scene.name}");

        if (scene.name == "Lobby")
        {
            isInGameScene = false;
            PlayBGM(0, 0);     // BGM도 안전하게 재생 가능
            StartCoroutine(LobbySetupCoroutine());
        }
        else if (scene.name == "InGame" || scene.name == "TestInGame")
        {
            isInGameScene = true;
            PlayBGM(0, 1); // 게임 씬 BGM
        }
    }
    /// <summary>
    ///  로비에서 사운드에 관련된 게임 오브젝트를 찾는 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator LobbySetupCoroutine()
    {
        yield return StartCoroutine(delayFindReference());

        addListnerInit(); // 이제 null 걱정 없음
        ApplyUI();
        //PlayBGM(0, 0);     // BGM도 안전하게 재생 가능
    }

    /// <summary>
    /// 씬 전환 시 Null 방지를 위한 코루틴
    /// </summary>
    /// <returns></returns>
    private IEnumerator delayFindReference()
    {
        Slider[] allSliders = Resources.FindObjectsOfTypeAll<Slider>();
        Toggle[] allToggles = Resources.FindObjectsOfTypeAll<Toggle>();

        while (true)
        {
            bgmSlider = AppleHungerTools.FindByName(allSliders, "BgmSliderbar");
            vfxSlider = AppleHungerTools.FindByName(allSliders, "VfxSliderbar");
            bgmToggle = AppleHungerTools.FindByName(allToggles, "BgmIcon");
            vfxToggle = AppleHungerTools.FindByName(allToggles, "VfxIcon");

            if (bgmSlider != null) Debug.Log("브금 슬라이더 찾음");
            if (vfxSlider != null) Debug.Log("효과 슬라이더 찾음");
            if (bgmToggle != null) Debug.Log("브금 토글 찾음");
            if (vfxToggle != null) Debug.Log("효과 토글 찾음");

            if (bgmSlider != null && vfxSlider != null && bgmToggle != null && vfxToggle != null)
                break;

            yield return null;
        }

        Debug.Log("audio UI 바인딩 완료!");
    }



    private void ApplyVolumes()
    {
        foreach (var bgm in bgmSources)
        {
            if (bgm != null)
                bgm.volume = isBGMMuted ? 0f : bgmVolume;
        }

        foreach (var vfx in vfxSources)
        {
            if (vfx != null)
                vfx.volume = isVFXMuted ? 0f : vfxVolume;
        }
    }

    private void ApplyUI()
    {
        if (bgmSlider != null) bgmSlider.value = bgmVolume;
        if (vfxSlider != null) vfxSlider.value = vfxVolume;

        if (bgmToggle != null)
        {
            bgmToggle.isOn = !isBGMMuted;

            var boardOff = bgmToggle.transform.Find("Board_Off");
            var boardOn = bgmToggle.transform.Find("Board_On");

            if (boardOff != null && boardOn != null)
            {
                boardOff.gameObject.SetActive(isBGMMuted);
                boardOn.gameObject.SetActive(!isBGMMuted);
            }
        }

        if (vfxToggle != null)
        {
            vfxToggle.isOn = !isVFXMuted;

            var boardOff = vfxToggle.transform.Find("Board_Off");
            var boardOn = vfxToggle.transform.Find("Board_On");

            if (boardOff != null && boardOn != null)
            {
                boardOff.gameObject.SetActive(isVFXMuted);
                boardOn.gameObject.SetActive(!isVFXMuted);
            }
        }
    }


    public void ToggleBGMMute(bool isOn)
    {
        isBGMMuted = !isOn; // 토글이 켜져있으면 음소거는 false
        ApplyVolumes();
        PlayerPrefs.SetInt(BGM_MUTE_KEY, isBGMMuted ? 1 : 0);

        // ✅ 이미지 제어
        Transform boardOff = bgmToggle.transform.Find("Board_Off");
        Transform boardOn = bgmToggle.transform.Find("Board_On");

        if (boardOff != null && boardOn != null)
        {
            boardOff.gameObject.SetActive(!isOn); // 꺼짐일 때만 켜짐
            boardOn.gameObject.SetActive(isOn);   // 켜짐일 때만 켜짐
        }
    }


    public void ToggleVFXMute(bool isOn)
    {
        isVFXMuted = !isOn;
        ApplyVolumes();
        PlayerPrefs.SetInt(VFX_MUTE_KEY, isVFXMuted ? 1 : 0);

        Transform boardOff = vfxToggle.transform.Find("Board_Off");
        Transform boardOn = vfxToggle.transform.Find("Board_On");

        if (boardOff != null && boardOn != null)
        {
            boardOff.gameObject.SetActive(!isOn);
            boardOn.gameObject.SetActive(isOn);
        }
    }


    public void OnBGMVolumeChanged(float value)
    {
        bgmVolume = value;
        if (!isBGMMuted)
        {
            foreach (var bgm in bgmSources)
            {
                if (bgm != null)
                    bgm.volume = value;
            }
        }
        PlayerPrefs.SetFloat(BGM_VOLUME_KEY, bgmVolume);
    }

    public void OnVFXVolumeChanged(float value)
    {
        vfxVolume = value;
        if (!isVFXMuted)
        {
            foreach (var vfx in vfxSources)
            {
                if (vfx != null)
                    vfx.volume = value;
            }
        }
        PlayerPrefs.SetFloat(VFX_VOLUME_KEY, vfxVolume);
    }

    /// <summary>
    /// 만약 오디오 출력하는 Source가 여러개일 경우에 VFX 출력을 위한 함수
    /// </summary>
    /// <param name="index"></param>
    /// <param name="clipIndex"></param>
    public void PlayVFX(int index, int clipIndex)
    {
        if (index >= 0 && index < vfxSources.Count && vfxSources[index] != null)
        {
            vfxSources[index].PlayOneShot(vfxClipList[clipIndex]);
        }
    }
    /// <summary>
    /// Sound Manager에 등록된 값중 0번 인덱스에 해당하는 값만 출력
    /// </summary>
    /// <param name="clipIndex"></param>
    public void PlayVFX(int clipIndex)
    {
        if (vfxSources.Count > 0 && vfxSources[0] != null)
        {
            vfxSources[0].PlayOneShot(vfxClipList[clipIndex]);
        }
    }

    public void PlayBGM(int index, int clipIndex)
    {
        for (int i = 0; i < bgmSources.Count; i++)
        {
            if (bgmSources[i] != null)
            {
                if (i == index)
                {
                    bgmSources[i].Stop(); // 기존 재생 중지
                    bgmSources[i].clip = bgmClipList[clipIndex];
                    bgmSources[i].loop = true;
                    bgmSources[i].Play();
                    Debug.Log($"BGM 재생: {bgmClipList[clipIndex].name}");
                }
                else
                {
                    bgmSources[i].Stop();
                }
            }
        }
    }

    private void LoadSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        vfxVolume = PlayerPrefs.GetFloat(VFX_VOLUME_KEY, 1f);
        isBGMMuted = PlayerPrefs.GetInt(BGM_MUTE_KEY, 0) == 1;
        isVFXMuted = PlayerPrefs.GetInt(VFX_MUTE_KEY, 0) == 1;

        Debug.Log($"[LoadSettings] bgmVolume: {bgmVolume}, mute? {isBGMMuted}, vfxVolume: {vfxVolume} , mute? {isVFXMuted}");

    }


}
