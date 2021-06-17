using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UILogic : MonoBehaviour
{
    [SerializeField] EventSystem canvasEventsystem;

    InputActionAsset iaa;
    InputActionMap playerControlsMap;
    InputActionMap UIControlsMap;
    InputAction escape;
    InputAction escapeUI;
    public GameObject options;
    public GameObject controller;
    public GameObject keyboard;
    bool controlsActive = false;

    [SerializeField] Camera startCamera;
    [SerializeField] Camera playCamera;

    [SerializeField] GameObject startCanvas;
    public List<GameObject> uiElements;
    bool startGotPressed = false;
    float timer = 0;

    public GameObject[] dummyTexts;
    bool optionGotSelectet = false;
    bool optionGotDeselectet = false;
    bool startcanvasDisabled = false;

    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(GameObject.FindGameObjectWithTag("PLAY"));

        iaa = ObjectManager.instance.pSM.actionAsset;
        playerControlsMap = iaa.FindActionMap("PlayerControls");
        UIControlsMap = iaa.FindActionMap("UIControls");
        escape = playerControlsMap.FindAction("Escape");
        escape.performed += context => ShowControls();
        escapeUI = UIControlsMap.FindAction("Escape");
        escapeUI.performed += context => Options();
        playerControlsMap.Disable();
        UIControlsMap.Enable();
    }

    private void Update()
    {
        DisableStartCanvas();
        Transitions();
    }

    void ShowControls()
    {
        /*UIControlsMap.Disable();
        playerControlsMap.Disable();

        if (controlsActive)
        {
            playerControlsMap.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            playerControlsMap.Enable();
            options.SetActive(false);
            controlsActive = false;
        }
        else
        {
            UIControlsMap.Enable();
            Cursor.lockState = CursorLockMode.None;
            playerControlsMap.Disable();
            options.SetActive(true);
            controlsActive = true;
        }*/
    }

    public void Play()
    {
        startGotPressed = true;

        /*playerControlsMap.Enable();
        Cursor.lockState = CursorLockMode.Locked;
        playerControlsMap.Enable();
        controls.SetActive(false);
        controlsActive = false;*/
    }

    void DisableStartCanvas()
    {
        if (startGotPressed)
        {
            timer += Time.deltaTime;

            //Vector3 designatedPosition = GameObject.Find("PLAY").GetComponent<RectTransform>().position;
            foreach (GameObject g in uiElements)
            {
                #region //positionchange
                /*if (g.tag != "PLAY")
                {
                    g.GetComponent<Image>().CrossFadeAlpha(0,.4f, false);
                    g.transform.position = Vector3.Lerp(g.transform.position, designatedPosition, .01f);
                }
                else
                {
                    g.transform.localScale = Vector3.Lerp(g.transform.localScale, new Vector3(g.transform.localScale.x * 1.5f, g.transform.localScale.y * 1.5f, g.transform.localScale.z * 1.5f), .01f);
                    g.GetComponent<Image>().CrossFadeAlpha(0, .6f, false);
                }*/
                #endregion
                if (g.GetComponent<Image>() != null)
                {
                    g.GetComponent<Image>().CrossFadeAlpha(0, .4f, false);
                }
                else
                {
                    g.SetActive(false);
                }
            }
            startCanvas.transform.position = Vector3.MoveTowards(startCanvas.transform.position, new Vector3(-1000f, 0, 0), 30f);
            startCamera.transform.position = Vector3.MoveTowards(startCamera.transform.position, playCamera.transform.position, .15f);
            startCamera.transform.rotation = Quaternion.Lerp(startCamera.transform.rotation, playCamera.transform.rotation, .1f);

            if (timer >= 2f)
            {
                timer = 0;
                startGotPressed = false;
                startCamera.enabled = false;
                startCanvas.SetActive(false);
                startcanvasDisabled = true;
                playerControlsMap.Enable();
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void Options()
    {
        if (options.activeSelf)
        {
            Back();
        }
        else
        {
            optionGotSelectet = true;
        }
    }

    void Transitions()
    {
        if (optionGotSelectet && startCanvas.activeSelf)
        {
            options.SetActive(true);
            options.transform.position = Vector3.MoveTowards(options.transform.position, new Vector3(0, 0, 0), 30f);
            startCanvas.transform.position = Vector3.MoveTowards(startCanvas.transform.position, new Vector3(1253f, 0, 0), 30f);

            if (Vector3.Distance(options.transform.position, new Vector3(0, 0, 0)) < 0.001f)
            {
                startCanvas.SetActive(false);
                EventSystem.current.SetSelectedGameObject(GameObject.FindGameObjectWithTag("BACK"));
                optionGotSelectet = false;
            }
        }
        if (optionGotSelectet && !startCanvas.activeSelf)
        {
            options.SetActive(true);
            options.transform.position = Vector3.MoveTowards(options.transform.position, new Vector3(0, 0, 0), 30f);

            if (Vector3.Distance(options.transform.position, new Vector3(0, 0, 0)) < 0.001f)
            {
                EventSystem.current.SetSelectedGameObject(GameObject.FindGameObjectWithTag("BACK"));
                optionGotSelectet = false;
            }
        }
        if (!startGotPressed && optionGotDeselectet && !startCanvas.activeSelf)
        {
            options.transform.position = Vector3.MoveTowards(options.transform.position, new Vector3(-1253f, 0, 0), 30f);

            if (Vector3.Distance(options.transform.position, new Vector3(-1253f, 0, 0)) < 0.001f)
            {
                foreach (GameObject g in dummyTexts)
                {
                    g.SetActive(false);
                }
                options.SetActive(false);
                optionGotDeselectet = false;
            }
        }
        if (optionGotDeselectet && !startCanvas.activeSelf && !startcanvasDisabled)
        {
            startCanvas.SetActive(true);
        }
        if (optionGotDeselectet && startCanvas.activeSelf)
        {
            options.transform.position = Vector3.MoveTowards(options.transform.position, new Vector3(-1253f, 0, 0), 30f);
            startCanvas.transform.position = Vector3.MoveTowards(startCanvas.transform.position, new Vector3(0, 0, 0), 30f);

            if (Vector3.Distance(startCanvas.transform.position, new Vector3(0, 0, 0)) < 0.001f)
            {
                foreach (GameObject g in dummyTexts)
                {
                    g.SetActive(false);
                }
                options.SetActive(false);
                EventSystem.current.SetSelectedGameObject(GameObject.FindGameObjectWithTag("PLAY"));
                Debug.Log("PLAY");
                optionGotDeselectet = false;
            }
        }
    }

    public void Back()
    {
        optionGotDeselectet = true;
    }

    public void AudioSettings()
    {
        foreach (GameObject g in dummyTexts)
        {
            if (g.name == "Audio")
            {
                g.gameObject.SetActive(true);
            }
            else
            {
                g.SetActive(false);
            }
        }
    }

    public void Controls()
    {
        foreach (GameObject g in dummyTexts)
        {
            if (g.name == "Controls")
            {
                g.gameObject.SetActive(true);
            }
            else
            {
                g.SetActive(false);
            }
        }
    }

    public void Visuals()
    {
        foreach (GameObject g in dummyTexts)
        {
            if (g.name == "Visuals")
            {
                g.gameObject.SetActive(true);
            }
            else
            {
                g.SetActive(false);
            }
        }
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
