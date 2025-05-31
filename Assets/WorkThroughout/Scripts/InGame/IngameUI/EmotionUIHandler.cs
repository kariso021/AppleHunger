using UnityEngine;
using System.Collections;
using Unity.Netcode;
using static TMPro.Examples.ObjectSpin;



public class EmotionUIHandler : MonoBehaviour
{
    public EmotionUI emotionUI;

    public void PlayEmotion(EmotionType emotion)
    {
        string code = GetEmotionCode(emotion);
        PlayEmotionLocally(emotion);

        //네트워크
        PlayerDataManager.Instance.SendEmotionServerRpc(code);
    }

    private void PlayEmotionLocally(EmotionType emotion)
    {
        string EmotionString = GetEmotionCode(emotion);
        AddressableManager.Instance.LoadImageFromGroup(EmotionString, emotionUI.player_ShowEmotionImage);
        emotionUI.player_ShowEmotionPanel.SetActive(true);

        StartCoroutine(HideEmotionAfterSeconds(1f));
    }

    private IEnumerator HideEmotionAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        emotionUI.player_ShowEmotionPanel.SetActive(false);
    }

    private string GetEmotionCode(EmotionType emotion)
    {
        // 프로필 아이콘 3번째 자리 추출
        string icon = SQLiteManager.Instance.player.profileIcon;
        string n = (icon != null && icon.Length >= 3)
            ? icon[2].ToString()
            : "0";

        Debug.Log($"[Emotion] EM TEST ___ {icon} and {n}");

        int emotionNum;
        switch (emotion)
        {
            case EmotionType.Angry: emotionNum = 1; break;
            case EmotionType.Laugh: emotionNum = 2; break;
            case EmotionType.Sad: emotionNum = 3; break;
            default: emotionNum = 0; break;
        }

        // 최종조합
        return $"3{n}{emotionNum}";
    }


}

public enum EmotionType
{
    Angry,
    Laugh,
    Sad
}
