using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyAssets : MonoBehaviour {



    public static LobbyAssets Instance { get; private set; }


    [SerializeField] private Sprite marineSprite;
    [SerializeField] private Sprite ninjaSprite;
    [SerializeField] private Sprite zombieSprite;


    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite(LobbyManage.PlayerCharacter playerCharacter) {
        switch (playerCharacter) {
            default:
            case LobbyManage.PlayerCharacter.Marine:   return marineSprite;
            case LobbyManage.PlayerCharacter.Ninja:    return ninjaSprite;
            case LobbyManage.PlayerCharacter.Zombie:   return zombieSprite;
        }
    }

}