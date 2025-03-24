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

    private long patchSize;
    private Dictionary<string,int> patchMap = new Dictionary<string,int>();
    // Start is called before the first frame update
    void Start()
    {
    }

    IEnumerator InitAddresaable()
    {
        var init = Addressables.InitializeAsync();
        yield return init;
    }
    #region Check Update
    IEnumerator CheckUpdateFiles()
    {
        var labels = new List<string>() { iconLabel.labelString, boardLabel.labelString };
        patchSize = 0L;

        Debug.Log($"[Addressables] Current BuildTarget: {Application.platform}");
        Debug.Log($"[Addressables] RemoteLoadPath: {Addressables.RuntimePath}");

        foreach (var label in labels)
        {
            if (string.IsNullOrEmpty(label))
            {
                Debug.LogError($"[Addressables] 라벨이 존재하지 않음: {label}");
                continue;
            }

            var handel = Addressables.GetDownloadSizeAsync(label);
            yield return handel;

            if (handel.Status == AsyncOperationStatus.Failed)
            {
                Debug.LogError($"[Addressables] 다운로드 크기 확인 실패: {label}");
                continue;
            }

            Debug.Log($"[Addressables] {label} 다운로드 크기: {handel.Result} bytes");

            patchSize += handel.Result;
        }

        Debug.Log($"[Addressables] 최종 패치 크기: {patchSize} bytes");

        if (patchSize > 0)
        {
            waitMessage.SetActive(false);
            downMessage.SetActive(true);
            sizeInfoText.text = GetFileSize(patchSize);
        }
        else
        {
            downValText.text = " 100%";
            downSlider.value = 1f;
            yield return new WaitForSeconds(2f);
            NavManager.LoadScene("MainScene");
        }
    }


    private string GetFileSize(long byteCnt)
    {
        string size = "0 Bytes";

        if(byteCnt >= 1073741824.0 )
        {
            size = string.Format("{0:##.##}", byteCnt / 1073741824.0) + " GB";
        }
        else if (byteCnt >= 1048576.0)
        {
            size = string.Format("{0:##.##}", byteCnt / 1048576.0) + " MB";
        }
        else if (byteCnt >= 1024.0)
        {
            size = string.Format("{0:##.##}", byteCnt / 1024.0) + " KB";
        }
        else if(byteCnt > 0 && byteCnt < 1024.0)
        {
            size = byteCnt.ToString() + " Bytes";
        }
        return size;
    }
    #endregion
    #region Down
    public void Button_DownLoad()
    {
        StartCoroutine(PatchFiles());
    }

    IEnumerator PatchFiles()
    {
        var labels = new List<string>() { iconLabel.labelString, boardLabel.labelString };


        foreach (var label in labels)
        {
            var handel = Addressables.GetDownloadSizeAsync(label);

            yield return handel;

            if(handel.Result != decimal.Zero)
            {
                StartCoroutine(DownLoadLabel(label));
            }

        }

        Debug.Log($"PATCHSIZE : {patchSize}");

        yield return CheckDownload();
    }

    IEnumerator DownLoadLabel(string label)
    {
        patchMap.Add(label, 0);

        var handle = Addressables.DownloadDependenciesAsync(label, true);

        while (!handle.IsDone)
        {
            patchMap[label] = (int)handle.GetDownloadStatus().DownloadedBytes;
            yield return new WaitForEndOfFrame();
        }

        patchMap[label] = (int)handle.GetDownloadStatus().TotalBytes;
        Addressables.Release(handle);
    }

    IEnumerator CheckDownload()
    {
        var total = 0f;
        downValText.text = "0 %";

        // 🔹 1️⃣ patchSize == 0 체크하여 division by zero 방지
        if (patchSize <= 0)
        {
            Debug.LogError("[Addressables] patchSize 값이 0이므로 다운로드 진행 불가");
            downSlider.value = 1f;
            downValText.text = "100%";
            yield break;
        }

        while (true)
        {
            total = patchMap.Sum(tmp => tmp.Value);

            // 🔹 2️⃣ patchMap 값이 음수인지 확인
            if (patchMap.Any(kvp => kvp.Value < 0))
            {
                Debug.LogError("[Addressables] patchMap에 음수 값이 포함됨!");
                yield break;
            }

            if (patchSize > 0)
            {
                downSlider.value = Mathf.Clamp01(total / (float)patchSize);
                downValText.text = $"{(int)(downSlider.value * 100)} %";
            }

            Debug.Log($"[Addressables] 다운로드 진행률: {downSlider.value * 100}% (Total: {total} / PatchSize: {patchSize})");

            if (total >= patchSize) // ✅ >= 조건으로 변경하여 확실하게 체크
            {
                downValText.text = "100%";
                downSlider.value = 1f;
                SceneManager.LoadScene("Lobby");
                break;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void OnDownloadCheck()
    {
        waitMessage.SetActive(true);
        downMessage.SetActive(false);

        StartCoroutine(InitAddresaable());
        StartCoroutine(CheckUpdateFiles());
    }
    #endregion
}
