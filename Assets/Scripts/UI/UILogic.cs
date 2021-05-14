using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UILogic : MonoBehaviour
{
    public InputActionAsset iaa;
    InputActionMap playerControlsMap;
    InputAction escape;
    public GameObject controls;
    bool controlsActive = false;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerControlsMap = iaa.FindActionMap("PlayerControls");
        escape = playerControlsMap.FindAction("Escape");
        escape.performed += context => ShowControls();
    }

    void ShowControls()
    {
        if (controlsActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            controls.SetActive(false);
            controlsActive = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            controls.SetActive(true);
            controlsActive = true;
        }
    }

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controls.SetActive(false);
        controlsActive = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
