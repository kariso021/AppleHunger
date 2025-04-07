using UnityEngine;
using static TMPro.Examples.ObjectSpin;

public class EmotionUIHandler : MonoBehaviour
{
    public void PlayEmotion(EmtionType emotion)
    {
        PlayEmotionLocally(emotion);

        // 상대방에게도 전달 (네트워크 호출)
        SendEmotionToOthers(emotion);
    }

    private void PlayEmotionLocally(EmtionType emotion)
    {
       
    }

    private void SendEmotionToOthers(EmtionType emotion)
    {
        // 네트워크 감정 전달
        //NetworkEmotionSender.Instance.SendEmotion(emotion);
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
