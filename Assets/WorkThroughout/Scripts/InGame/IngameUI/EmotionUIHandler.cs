using UnityEngine;
using static TMPro.Examples.ObjectSpin;

public class EmotionUIHandler : MonoBehaviour
{
    public void PlayEmotion(EmtionType emotion)
    {
        PlayEmotionLocally(emotion);

        // ���濡�Ե� ���� (��Ʈ��ũ ȣ��)
        SendEmotionToOthers(emotion);
    }

    private void PlayEmotionLocally(EmtionType emotion)
    {
       
    }

    private void SendEmotionToOthers(EmtionType emotion)
    {
        // ��Ʈ��ũ ���� ����
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
