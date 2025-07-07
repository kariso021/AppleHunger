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
    private Dictionary<string, long> patchMap = new Dictionary<string, long>();

    private void Awake()
    {
        StartCoroutine(InitAddressable());
    }

    IEnumerator InitAddressable()
    {
        bool done = false;

        Addressables.InitializeAsync().Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("Addressables 초기화 성공");
                isInitialized = true;
            }
            else
            {
                Debug.LogError("Addressables 초기화 실패");
                if (handle.OperationException != null)
                    Debug.LogError($"예외: {handle.OperationException.Message}");
            }

            Addressables.Release(handle);
            done = true;
        };

        while (!done)
            yield return null;
    }

    public IEnumerator CheckUpdateFiles()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) // not connect internet
        {
            PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "인터넷 연결이 되어있지 않습니다. \n" + "인터넷 연결 후 게임을 재실행해주세요.";
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
            PopupManager.Instance.warningPopup.GetComponent <ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => Application.Quit());
            Debug.Log("[Network] Network is not available");

            yield break;
        }
        var labels = new List<string> { iconLabel.labelString, boardLabel.labelString, emojiLabel.labelString };

        patchSize = 0;
        patchMap.Clear();
        bool checkDone = false;

        foreach (var label in labels)
        {
            if (string.IsNullOrEmpty(label)) continue;

            checkDone = false;
            Addressables.GetDownloadSizeAsync(label).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result > 0)
                {
                    patchSize += handle.Result;
                }

                Addressables.Release(handle);
                checkDone = true;
            };

            while (!checkDone)
                yield return null;
        }

        Debug.Log($"[Down] patch size : {patchSize}");

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
            yield return new WaitForSeconds(1f);
            SceneManager.LoadScene("TestLobby");
        }
    }

    public void Button_Download()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable) // not connect internet
        {
            PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "인터넷 연결이 되어있지 않습니다. \n" + "인터넷 연결 후 게임을 재실행해 로그인을 시도해주세요.";
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_cancel.gameObject.SetActive(false);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => Application.Quit());
            Debug.Log("[Network] Network is not available");
        }
        else if (Application.internetReachability == NetworkReachability.ReachableViaCarrierDataNetwork) // lte,3g
        {
            PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "데이터로 연결되어 있습니다. 정말 다운받으시겠습니까?";
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
            PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => StartCoroutine(DownloadAll()));
            Debug.Log("[Network] Network is available");
            return;
        }
        else
            StartCoroutine(DownloadAll());
    }

    IEnumerator DownloadAll()
    {
        var labels = new List<string> { iconLabel.labelString, boardLabel.labelString, emojiLabel.labelString };
        StartCoroutine(UpdateProgress());

        foreach (var label in labels)
        {
            yield return DownloadWithRetry(label, 3);
        }

        SceneManager.LoadScene("TestLobby");
    }

    IEnumerator DownloadWithRetry(string label, int maxRetry)
    {
        patchMap[label] = 0;
        int attempt = 0;
        bool success = false;

        while (attempt < maxRetry && !success)
        {
            bool downloadDone = false;

            Addressables.DownloadDependenciesAsync(label, true).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    patchMap[label] = handle.GetDownloadStatus().TotalBytes;
                    Debug.Log($"[Down] 다운로드 성공: {label}");
                    success = true;
                }
                else
                {
                    Debug.LogWarning($"[Down] 다운로드 실패: {label} (시도 {attempt + 1}/{maxRetry})");
                }

                Addressables.Release(handle);
                downloadDone = true;
            };

            while (!downloadDone)
                yield return null;

            if (!success)
            {
                attempt++;
                yield return new WaitForSeconds(1f);
            }
        }

        if (!success)
        {
            Debug.LogError($"[Down] {label} 다운로드 실패 - 최대 재시도 도달");
        }
    }

    IEnumerator UpdateProgress()
    {
        downValText.text = "0%";

        while (true)
        {
            long totalDownloaded = patchMap.Values.Sum();
            downSlider.value = Mathf.Clamp01(totalDownloaded / (float)patchSize);
            downValText.text = $"{(int)(downSlider.value * 100)}%";

            if (totalDownloaded >= patchSize)
            {
                downSlider.value = 1f;
                downValText.text = "100%";
                yield break;
            }

            yield return null;
        }
    }

    private string GetFileSize(long byteCount)
    {
        if (byteCount >= 1073741824) return $"{byteCount / 1073741824.0:0.##} GB";
        if (byteCount >= 1048576) return $"{byteCount / 1048576.0:0.##} MB";
        if (byteCount >= 1024) return $"{byteCount / 1024.0:0.##} KB";
        return $"{byteCount} Bytes";
    }
}
