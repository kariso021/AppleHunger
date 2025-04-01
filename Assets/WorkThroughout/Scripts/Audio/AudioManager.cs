using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
            PlayBGM(0, 0); // 로비 BGM
        }
        else if (scene.name == "InGame")
        {
            isInGameScene = true;
            PlayBGM(0, 1); // 게임 씬 BGM
        }

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

        if (bgmToggle != null) bgmToggle.isOn = !isBGMMuted;
        if (vfxToggle != null) vfxToggle.isOn = !isVFXMuted;
    }

    public void ToggleBGMMute(bool isOn)
    {
        isBGMMuted = !isOn; // 토글이 켜져있으면 음소거 false
        ApplyVolumes();
        PlayerPrefs.SetInt(BGM_MUTE_KEY, isBGMMuted ? 1 : 0);
    }

    public void ToggleVFXMute(bool isOn)
    {
        isVFXMuted = !isOn;
        ApplyVolumes();
        PlayerPrefs.SetInt(VFX_MUTE_KEY, isVFXMuted ? 1 : 0);
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

    public void PlayVFX(int index, int clipIndex)
    {
        if (index >= 0 && index < vfxSources.Count && vfxSources[index] != null)
        {
            vfxSources[index].PlayOneShot(vfxClipList[clipIndex]);
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
