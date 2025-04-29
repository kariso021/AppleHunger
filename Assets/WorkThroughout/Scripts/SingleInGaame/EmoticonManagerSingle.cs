using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmoticonManagerSingle : MonoBehaviour
{
    [SerializeField] private GameObject EmoticonPanel;
    private bool ImoticonPanelActive;

    private void Start()
    {
        ImoticonPanelActive = false;
    }




    public void ToggleEmotion()
   {
        if (ImoticonPanelActive == false)
        {
            EmoticonPanel.SetActive(true);
            Debug.Log("Emoticon panel shown");
            ImoticonPanelActive = true;
        }
        else
        {
            EmoticonPanel.SetActive(false);
            ImoticonPanelActive = false;
        }
  
    }



}
