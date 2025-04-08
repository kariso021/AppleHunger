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
        Color color = GetEmotionColor(emotion);

        emotionUI.player_ShowEmotionImage.color = color;
        emotionUI.player_ShowEmotionPanel.SetActive(true);

        StartCoroutine(HideEmotionAfterSeconds(1f));
    }

    private IEnumerator HideEmotionAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        emotionUI.player_ShowEmotionPanel.SetActive(false);
    }

    private Color GetEmotionColor(EmtionType emotion)
    {
        switch (emotion)
        {
            case EmtionType.Taunt: return Color.yellow;
            case EmtionType.Laugh: return Color.red;
            case EmtionType.Clap: return Color.green;
            default: return Color.white;
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
