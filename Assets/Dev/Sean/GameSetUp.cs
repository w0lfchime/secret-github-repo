using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.UI;

public class GameSetUp : MonoBehaviour
{
    public List<GameObject> players;
    public List<Texture> playerIcons;
    public SliderHandlers sliderHandlers;
    public CameraPivotSpringFollow cameraPivotSpringFollow;
    public MirrorSpaceFollowerXZ mirrorSpaceFollowerXZ;
    public MirrowFollow mirrowFollow;
    public FrameSprayEmitter frameSprayEmitter; 
    public ItemSelectionUI itemSelectionUI;
    public RawImage playerIcon;
    private GameData gameData;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameData = GameObject.Find("GameData").GetComponent<GameData>();

        GameObject newPlayer = GameObject.Instantiate(players[gameData.charNum]);

        newPlayer.transform.position = new Vector3(1.09f, -8.75f, -9.41f);

        sliderHandlers.pc = newPlayer.GetComponent<PlayerController>();
        frameSprayEmitter.pc = newPlayer.GetComponent<PlayerController>();
        frameSprayEmitter.enabled = true;
        itemSelectionUI.pc = newPlayer.GetComponent<PlayerController>();
        itemSelectionUI.hc = newPlayer.GetComponent<AlexHammerCopy>();
        mirrorSpaceFollowerXZ.target = newPlayer.transform;
        cameraPivotSpringFollow.target = newPlayer.transform;
        // mirrowFollow.target = newPlayer.transform;

        playerIcon.texture = playerIcons[gameData.charNum];
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
