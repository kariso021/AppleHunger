using UnityEngine;
using System.Collections;
using Unity.Netcode;



public class EmotionUIHandler : MonoBehaviour
{
    public EmotionUI emotionUI;

    public void PlayEmotion(EmtionType emotion)
    {
        PlayEmotionLocally(emotion);

        //네트워크
        PlayerDataManager.Instance.SendEmotionServerRpc(emotion, NetworkManager.Singleton.LocalClientId);
    }

    private void PlayEmotionLocally(EmtionType emotion)
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

    private string GetEmotionCode(EmtionType emotion)
    {
        switch (emotion)
        {
            case EmtionType.Taunt: return "306";
            case EmtionType.Laugh: return "305";
            case EmtionType.Clap: return "303";
            default: return "301";
        }
    }
}

public enum EmtionType
{
    Taunt,
    Laugh,
    Clap,
    Angry,
    Sad,
}
