using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UILogic : MonoBehaviour
{
    InputActionAsset iaa;
    InputActionMap playerControlsMap;
    InputActionMap UIControlsMap;
    InputAction escape;
    InputAction escapeUI;
    public GameObject controls;
    public GameObject controller;
    public GameObject keyboard;
    bool controlsActive = false;

    private void Start()
    {
        iaa = ObjectManager.instance.pSM.actionAsset;
        playerControlsMap = iaa.FindActionMap("PlayerControlsNewSliding");
        UIControlsMap = iaa.FindActionMap("UIControls");
        escape = playerControlsMap.FindAction("Escape");
        escape.performed += context => ShowControls();
        escapeUI = UIControlsMap.FindAction("Escape");
        escapeUI.performed += context => ShowControls();
        playerControlsMap.Disable();
        UIControlsMap.Enable();
    }

    void ShowControls()
    {
        UIControlsMap.Disable();
        playerControlsMap.Disable();

        if (controlsActive)
        {
            playerControlsMap.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            playerControlsMap.Enable();
            controls.SetActive(false);
            controlsActive = false;
        }
        else
        {
            UIControlsMap.Enable();
            Cursor.lockState = CursorLockMode.None;
            playerControlsMap.Disable();
            controls.SetActive(true);
            controlsActive = true;
        }
    }

    public void Resume()
    {
        playerControlsMap.Enable();
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
