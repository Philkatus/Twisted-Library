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
    public GameObject controller;
    public GameObject keyboard;
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
            playerControlsMap.Enable();
            controls.SetActive(false);
            controlsActive = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            playerControlsMap.Disable();
            controls.SetActive(true);
            controlsActive = true;
        }
    }

    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerControlsMap.Enable();
        controls.SetActive(false);
        controlsActive = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ShowControllerControls()
    {
        controller.SetActive(true);
        keyboard.SetActive(false);
    }

    public void ShowKeyboardControls()
    {
        controller.SetActive(false);
        keyboard.SetActive(true);
    }
}
