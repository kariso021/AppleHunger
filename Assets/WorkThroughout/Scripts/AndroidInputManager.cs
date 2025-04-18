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
            if (PopupManager.Instance != null && PopupManager.Instance.activePopup != null)
            {
                // �˾��� ���� ������ �˾� �ݱ�
                PopupManager.Instance.ClosePopup();
            }
            else
            {
                // �˾��� ���� �� �� ���� ����
                if (Time.time - lastBackPressedTime < backPressInterval)
                {
                    // 2�� �̳� �� ��° ���� �� ���� ����
                    Application.Quit();
                }
                else
                {
                    // ù ��° ���� �� Toast�� ��� ���� ����
                    AndroidToast.ShowToast("�ڷΰ��� ��ư�� �� �� �� ������ ����˴ϴ�.");
                    lastBackPressedTime = Time.time;
                }
            }
        }
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
