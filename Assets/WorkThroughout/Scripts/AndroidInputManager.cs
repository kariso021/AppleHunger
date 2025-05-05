using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AndroidInputManager : MonoBehaviour
{
    private static AndroidInputManager instance;
    public static AndroidInputManager Instance => instance;

    private float lastBackPressedTime;
    private float backPressInterval = 2f; // 2�� �̳� �� �� ������ ����

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) // Android �ڷΰ���
        {
            Debug.Log("[Android] return input");
            if (PopupManager.Instance != null && PopupManager.Instance.activePopup != null)
            {
                // �˾��� ���� ������ �˾� �ݱ�
                PopupManager.Instance.ClosePopup();
                Debug.Log("[Android] return input to CLOSE POPUP");
            }
            else if(SceneManager.GetActiveScene().name == "TestInGame")
            {
                PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "�̱� �÷��̸� �׸��ΰ� �κ�� ���ư��ðڽ��ϱ�?";
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => SceneManager.LoadScene("TestLobby"));
                Debug.Log("[Android] return input to SHOW POPUPnsfhe");
            }
            else
            {
                // �˾��� ���� �� �� ���� ����
                PopupManager.Instance.ShowPopup(PopupManager.Instance.warningPopup);
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().config.text = "���� ������ �����Ͻðڽ��ϱ�?";
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.RemoveAllListeners();
                PopupManager.Instance.warningPopup.GetComponent<ModalPopup>().btn_confirm.onClick.AddListener(() => Application.Quit());
                Debug.Log("[Android] return input to SHOW POPUPnsfhe");
                //if (Time.time - lastBackPressedTime < backPressInterval)
                //{
                //    // 2�� �̳� �� ��° ���� �� ���� ����
                //    Application.Quit();
                //}
                //else
                //{
                //    // ù ��° ���� �� Toast�� ��� ���� ����
                //    AndroidToast.ShowToast("�ڷΰ��� ��ư�� �� �� �� ������ ����˴ϴ�.");
                //    lastBackPressedTime = Time.time;
                //}
            }
        }
    }

    public void RestartApp()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
    using (var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
    using (var packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager"))
    {
        string packageName = currentActivity.Call<string>("getPackageName");
        AndroidJavaObject launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);
        launchIntent.Call<AndroidJavaObject>("addFlags", 0x10000000); // FLAG_ACTIVITY_NEW_TASK

        currentActivity.Call("startActivity", launchIntent);

        currentActivity.Call("finishAffinity"); // ���� ��Ƽ��Ƽ ���� ����
    }
#endif
    }

}

public static class AndroidToast
{
    public static void ShowToast(string message)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            if (activity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>("makeText", activity, message, toastClass.GetStatic<int>("LENGTH_SHORT"));
                    toast.Call("show");
                }));
            }
        }
#endif
    }
}
