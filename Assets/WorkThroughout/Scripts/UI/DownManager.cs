using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DownManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject waitMessage;
    public GameObject downMessage;
    public TMP_Text sizeInfoText;
    public Slider downSlider;
    public TMP_Text downValText;

    [Header("Label")]
    public AssetLabelReference iconLabel;
    public AssetLabelReference boardLabel;
    public AssetLabelReference emojiLabel;

    private long patchSize;
    private Dictionary<string, int> patchMap = new Dictionary<string, int>();

    private void Start()
    {
        StartCoroutine(InitAddressable());
    }

    IEnumerator InitAddressable()
    {
        var init = Addressables.InitializeAsync();
        yield return init;
    }

    #region Check Update
    public IEnumerator CheckUpdateFiles()
    {
        var labels = new List<string>
        {
            iconLabel.labelString,
            boardLabel.labelString,
            emojiLabel.labelString
        };

        patchSize = 0L;

        foreach (var label in labels)
        {
            if (string.IsNullOrEmpty(label)) continue;

            var handle = Addressables.GetDownloadSizeAsync(label);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result > 0)
            {
                patchSize += handle.Result;
            }

            Addressables.Release(handle);
        }

        if (patchSize > 0)
        {
            waitMessage.SetActive(false);
            downMessage.SetActive(true);
            sizeInfoText.text = GetFileSize(patchSize);
        }
        else
        {
            downSlider.value = 1f;
            downValText.text = "100%";
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("Lobby");
        }
    }

    private string GetFileSize(long byteCount)
    {
        if (byteCount >= 1073741824) return $"{byteCount / 1073741824.0:0.##} GB";
        if (byteCount >= 1048576) return $"{byteCount / 1048576.0:0.##} MB";
        if (byteCount >= 1024) return $"{byteCount / 1024.0:0.##} KB";
        return $"{byteCount} Bytes";
    }
    #endregion

    #region Download
    public void Button_Download()
    {
        StartCoroutine(PatchFiles());
    }

    IEnumerator PatchFiles()
    {
        var labels = new List<string> { iconLabel.labelString, boardLabel.labelString };

        foreach (var label in labels)
        {
            var sizeHandle = Addressables.GetDownloadSizeAsync(label);
            yield return sizeHandle;

            if (sizeHandle.Status == AsyncOperationStatus.Succeeded && sizeHandle.Result > 0)
            {
                yield return StartCoroutine(DownloadLabel(label));
            }

            Addressables.Release(sizeHandle);
        }

        yield return CheckDownload();
    }

    IEnumerator DownloadLabel(string label)
    {
        patchMap[label] = 0;

        int retryCount = 0;
        int maxRetryCount = 3;

        while (retryCount < maxRetryCount)
        {
            var handle = Addressables.DownloadDependenciesAsync(label, true);

            while (!handle.IsDone)
            {
                if (handle.IsValid())
                {
                    var status = handle.GetDownloadStatus();
                    patchMap[label] = (int)status.DownloadedBytes;

                }

                yield return null;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                patchMap[label] = (int)handle.GetDownloadStatus().TotalBytes;
                Addressables.Release(handle);
                yield break; // ✅ 다운로드 성공 → 코루틴 종료
            }
            else
            {
                Debug.LogWarning($"[Addressables] 다운로드 실패 - 재시도 {retryCount + 1}/{maxRetryCount} : {label}");
                Addressables.Release(handle);
                retryCount++;

                yield return new WaitForSeconds(1f); // 재시도 대기시간
            }
        }

        Debug.LogError($"[Addressables] 다운로드 3회 재시도 실패: {label}");
    }


    IEnumerator CheckDownload()
    {
        downValText.text = "0 %";

        if (patchSize <= 0)
        {
            downSlider.value = 1f;
            downValText.text = "100%";
            yield break;
        }

        while (true)
        {
            float total = patchMap.Sum(kvp => kvp.Value);

            if (patchMap.Any(kvp => kvp.Value < 0))
            {
                Debug.LogError("[Addressables] patchMap에 음수 값 포함!");
                yield break;
            }

            downSlider.value = Mathf.Clamp01(total / patchSize);
            downValText.text = $"{(int)(downSlider.value * 100)} %";

            if (total >= patchSize)
            {
                downSlider.value = 1f;
                downValText.text = "100%";
                SceneManager.LoadScene("Lobby");
                yield break;
            }

            yield return null;
        }
    }

    public void OnDownloadCheck()
    {
        waitMessage.SetActive(true);
        downMessage.SetActive(false);

        StartCoroutine(InitAddressable());
        StartCoroutine(CheckUpdateFiles());
    }
    #endregion
}
