using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenshotHotKey : MonoBehaviour
{

    public bool takeScreen;

    public InputActionAsset actionAsset;
    InputActionMap playerControlsMap;
    InputAction takeScreenButton;

    public static ScreenshotHotKey Instance;
    void Awake()
    {

        if (Instance == null)
        {

            Instance = this;
            DontDestroyOnLoad(this.gameObject);

            //Rest of your Awake code

        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        playerControlsMap = actionAsset.FindActionMap("PlayerControls");
        playerControlsMap.Enable();
        takeScreenButton = playerControlsMap.FindAction("TakeScreenshot");
        takeScreenButton.Enable();
        takeScreenButton.performed += context => TakeHiResShot();
    }


    void Update()
    {
       // if(takeScreenButton.triggered)
        //{
      //      TakeHiResShot();
       // }
    }

    public void TakeHiResShot()
    {
        Debug.Log("Taking Screenshot");
        takeScreen = true;
    }
}
