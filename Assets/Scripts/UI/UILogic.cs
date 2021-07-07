using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cinemachine;

public class UILogic : MonoBehaviour
{
    [SerializeField] EventSystem canvasEventsystem;

    InputActionAsset inputActionAsset;
    InputActionMap playerControlsMap;
    InputActionMap UIControlsMap;
    InputAction escape;
    InputAction escapeUI;
    InputAction showMoreOptions;
    InputAction back;

    [SerializeField] Camera startCamera;
    [SerializeField] Camera playCamera;
    [SerializeField] Image controlsImage;
    [SerializeField] GameObject startCanvas;
    [SerializeField] Toggle invertedSlidingToggle;
    [SerializeField] Toggle jumpForLadderPushToggle;

    public GameObject options, inGameUI;
    public GameObject controller;
    public GameObject keyboard;
    public GameObject[] optionsContent;
    public List<GameObject> uiElements;
    public List<Button> startCanvasButtons;
    public Vector3 inactiveSize;
    public Vector3 activeSize;

    float timer;
    float uiAlpha;
    bool startGotPressed;
    bool controlsActive;
    bool moreOptionsSelected;
    bool optionGotSelected;
    bool optionGotDeselected;
    bool startcanvasDisabled;

    private void Start()
    {
        ObjectManager.instance.uILogic = this;
        EventSystem.current.SetSelectedGameObject(GameObject.FindGameObjectWithTag("PLAY"));

        inputActionAsset = ObjectManager.instance.pSM.actionAsset;
        playerControlsMap = inputActionAsset.FindActionMap("PlayerControls");
        escape = playerControlsMap.FindAction("Escape");
        UIControlsMap = inputActionAsset.FindActionMap("UIControls");
        //escape.performed += context => ShowControls();
        escapeUI = UIControlsMap.FindAction("Escape");
        back = UIControlsMap.FindAction("Back");
        showMoreOptions = UIControlsMap.FindAction("MoreOptions");
        showMoreOptions.performed += context => ShowMoreOptions();
        back.performed += context => Back();
        escapeUI.performed += context => Options();
        playerControlsMap.Disable();
        UIControlsMap.Enable();
        uiAlpha = ObjectManager.instance.pSM.stats.alphaForTransparentUI;

        #region Set PlayerPrefs
        if (PlayerPrefs.GetInt("UseInvertedSliding", 0) == 1)
        {
            ObjectManager.instance.pSM.stats.useInvertedSliding = true;
            // TO DO: checkbox auf an setzen
        }
        else
        {
            ObjectManager.instance.pSM.stats.useInvertedSliding = false;
            // TO DO: checkbox auf aus setzen
        }

        if (PlayerPrefs.GetInt("UseJumpForLadderPush", 1) == 1)
        {
            ObjectManager.instance.pSM.stats.useJumpForLadderPush = true;
            // TO DO: checkbox auf an setzen
        }
        else
        {
            ObjectManager.instance.pSM.stats.useJumpForLadderPush = false;
            // TO DO: checkbox auf aus setzen
        }

        #endregion
    }

    private void Update()
    {
        DisableStartCanvas();
        Transitions();
    }

    public void Play()
    {
        startGotPressed = true;
    }

    void DisableStartCanvas()
    {
        if (startGotPressed)
        {
            inGameUI.SetActive(true);
            timer += Time.deltaTime;

            foreach (Button b in startCanvasButtons)
            {
                b.interactable = false;
            }
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

            if (timer >= .5f)
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
            optionGotSelected = true;
        }
    }

    public void DebugMessage()
    {
        Debug.LogError("options button");
    }

    void Transitions()
    {
        if (optionGotSelected && startCanvas.activeSelf)
        {
            options.SetActive(true);
            options.transform.position = Vector3.MoveTowards(options.transform.position, new Vector3(0, 0, 0), 30f);
            startCanvas.transform.position = Vector3.MoveTowards(startCanvas.transform.position, new Vector3(1253f, 0, 0), 30f);

            if (Vector3.Distance(options.transform.position, new Vector3(0, 0, 0)) < 0.001f)
            {
                startCanvas.SetActive(false);
                EventSystem.current.SetSelectedGameObject(GameObject.FindGameObjectWithTag("BACK"));
                optionGotSelected = false;
            }
        }
        if (optionGotSelected && !startCanvas.activeSelf)
        {
            options.SetActive(true);
            options.transform.position = Vector3.MoveTowards(options.transform.position, new Vector3(0, 0, 0), 30f);
            Time.timeScale = 0;

            if (Vector3.Distance(options.transform.position, new Vector3(0, 0, 0)) < 0.001f)
            {
                EventSystem.current.SetSelectedGameObject(GameObject.FindGameObjectWithTag("BACK"));
                optionGotSelected = false;
            }
        }
        if (!startGotPressed && optionGotDeselected && !startCanvas.activeSelf)
        {
            options.transform.position = Vector3.MoveTowards(options.transform.position, new Vector3(-1253f, 0, 0), 30f);
            Time.timeScale = 1;

            if (Vector3.Distance(options.transform.position, new Vector3(-1253f, 0, 0)) < 0.001f)
            {
                foreach (GameObject g in optionsContent)
                {
                    g.SetActive(false);
                }
                options.SetActive(false);
                optionGotDeselected = false;
            }
        }
        if (optionGotDeselected && !startCanvas.activeSelf && !startcanvasDisabled)
        {
            startCanvas.SetActive(true);
        }
        if (optionGotDeselected && startCanvas.activeSelf)
        {
            options.transform.position = Vector3.MoveTowards(options.transform.position, new Vector3(-1253f, 0, 0), 30f);
            startCanvas.transform.position = Vector3.MoveTowards(startCanvas.transform.position, new Vector3(0, 0, 0), 30f);

            if (Vector3.Distance(startCanvas.transform.position, new Vector3(0, 0, 0)) < 0.001f)
            {
                foreach (GameObject g in optionsContent)
                {
                    g.SetActive(false);
                }
                options.SetActive(false);
                EventSystem.current.SetSelectedGameObject(GameObject.FindGameObjectWithTag("PLAY"));
                optionGotDeselected = false;
            }
        }
    }

    public void Back()
    {
        if (moreOptionsSelected)
        {
            controlsImage.enabled = true;
            moreOptionsSelected = false;
            foreach (GameObject g in optionsContent)
            {
                g.SetActive(false);
            }
        }
        else
        {
            optionGotDeselected = true;
        }
    }

    public void ShowMoreOptions()
    {
        // schreib hier rein, was passieren soll, wenn mehr ooptions angezeigt werden sollen (toggles erschienen usw.)
        controlsImage.enabled = false;
        moreOptionsSelected = true;

        foreach (GameObject g in optionsContent)
        {
            g.SetActive(true);
        }
        EventSystem.current.SetSelectedGameObject(GameObject.FindGameObjectWithTag("FirstToggle"));
    }

    public void ShowLandmarkUI(GameObject firstLinkedUI, GameObject secondLinkedUI, GameObject thirdLinkedUI, GameObject groundUI)
    {
        ExtensionMethods.CrossFadeAlphaFixed(firstLinkedUI, uiAlpha, .2f);
        ExtensionMethods.CrossFadeAlphaFixed(secondLinkedUI, uiAlpha, .2f);
        ExtensionMethods.CrossFadeAlphaFixed(thirdLinkedUI, uiAlpha, .2f);
        ExtensionMethods.CrossFadeAlphaFixed(groundUI, uiAlpha, .2f);
    }

    public void OnChallengeFailed(GameObject linkedUI, string type)
    {
        // verstecke wieder alle switches und zahnräder, weil die challenge gefailt wurde
        if (type == "switch")
        {
            linkedUI.GetComponent<RectTransform>().localScale = inactiveSize;
            linkedUI.GetComponent<Slider>().value = .75f;
            linkedUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Slider>().value = .75f;
        }
        if (type == "cogwheel")
        {
            linkedUI.GetComponent<Slider>().value = .49f;
            linkedUI.GetComponent<RectTransform>().localScale = inactiveSize;
            linkedUI.transform.GetChild(0).GetComponent<Animator>().SetBool("WheelGotTriggered", false);
        }
    }

    public void OnComponentComplete(GameObject linkedUI, string type)
    {
        // immer wenn ein hebel umgelegt wird, wird das im jeweiligen hebel aufgerufen mit dem jeweiligen ui objekt (linkedUI)
        if (type == "switch")
        {
            linkedUI.transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, 40);
            linkedUI.GetComponent<Slider>().value = 1;
            linkedUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Slider>().value = 1;
        }
        if (type == "cogwheel")
        {
            //Debug.Log("tiurn on");

            linkedUI.transform.GetChild(0).GetComponent<Animator>().SetBool("WheelGotTriggered", true);
            linkedUI.GetComponent<Slider>().value = .83f;
        }
    }

    public void OnChallengeCompleteLandmark(GameObject linkedUI)
    {
        // wenn alle switches an sind, wird das aufgerufen (challenge complete), wird im landmark script aufgerufen für das dazu
        // gehörige element vom landmark, was nun leuchten soll
        ExtensionMethods.CrossFadeAlphaFixed(linkedUI, 1f, .1f);
    }

    public void SetLandmarkScaleToSmall(GameObject firstLinkedUI, GameObject secondLinkedUI, GameObject thirdLinkedUI, GameObject groundUI, float time)
    {
        // wenn ein challenge complete ist, soll das landmark ui wieder klein werden
        firstLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(activeSize, inactiveSize, time);
        secondLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(activeSize, inactiveSize, time);
        thirdLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(activeSize, inactiveSize, time);
        groundUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(activeSize, inactiveSize, time);
    }

    public void OnHideChallengeComponent(GameObject linkedUI, string type)
    {
        // hide component ui after challenge was completed or failed

        if (type == "switch")
        {
            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject, 0.0f, 0.5f);
            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject, 0.0f, 0.5f);
            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(1).transform.GetChild(0).gameObject, 0.0f, 0.5f);
            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(1).gameObject, 0.0f, 0.5f);
        }
        if (type == "cogwheel")
        {
            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(0).gameObject, 0.0f, 0.5f);
            linkedUI.transform.GetChild(0).GetComponent<Animator>().SetBool("WheelGotTriggered", false);
        }
    }

    public void OnChallengeStartedComponent(GameObject linkedUI, string type)
    {
        // show the UI items and set correct sizes
        // passiert nur ein mal, wenn die erste switch aktiviert wird bei der challenge
        if (type == "switch")
        {
            linkedUI.GetComponent<RectTransform>().localScale = inactiveSize;

            linkedUI.GetComponent<Slider>().value = .75f;
            linkedUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Slider>().value = .75f;
            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject, uiAlpha, .2f);
            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).gameObject, 1f, .2f);
            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(1).gameObject, uiAlpha, .2f);
            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(1).transform.GetChild(0).gameObject, 1f, .2f);
        }
        if (type == "cogwheel")
        {
            linkedUI.GetComponent<RectTransform>().localScale = inactiveSize;
            linkedUI.GetComponent<Slider>().value = .49f;

            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(0).gameObject, uiAlpha, .2f);
            ExtensionMethods.CrossFadeAlphaFixed(linkedUI.transform.GetChild(0).transform.GetChild(0).gameObject, 1f, .2f);
        }
    }

    public void SetLandmarkScaleToBig(GameObject firstLinkedUI, GameObject secondLinkedUI, GameObject thirdLinkedUI, GameObject groundUI, float time)
    {
        // mach dieses landmark größer, weil gerade deren challenge gemacht wird
        firstLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(inactiveSize, activeSize, time);
        secondLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(inactiveSize, activeSize, time);
        thirdLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(inactiveSize, activeSize, time);
        groundUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(inactiveSize, activeSize, time);
    }

    public void OnLandmarkComplete()
    {

    }


    public void UpdateComponentVisual(GameObject linkedUI, string type, float timer, float timeToCompleteComponents, bool turnOn)
    {
        // abhängig ob switch oder zahnrad
        if (type == "switch")
        {
            // nur zum "Anschalten", geht schnell runter
            if (turnOn)
            {
                linkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(inactiveSize, activeSize, timer);
                linkedUI.transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 40, timer));

                linkedUI.GetComponent<Slider>().value = 1;
                linkedUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Slider>().value = 1;
            }
            else
            {
                // geht dann hoch abhängig von timeToCompleteComponents (im Inspektor von der Challenge gesetzt)
                linkedUI.transform.GetChild(0).transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(40, 0, ExtensionMethods.Remap(timer, 0, timeToCompleteComponents, 0, 1)));
                linkedUI.GetComponent<Slider>().value = Mathf.Lerp(1f, .75f, ExtensionMethods.Remap(timer, 0, timeToCompleteComponents, 0, 1));
                linkedUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Slider>().value = Mathf.Lerp(1f, .75f, ExtensionMethods.Remap(timer, 0, timeToCompleteComponents, 0, 1));
            }
        }
        if (type == "cogwheel")
        {
            if (turnOn)
            {
                linkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(inactiveSize, activeSize, timer);
                linkedUI.transform.GetChild(0).GetComponent<Animator>().SetBool("WheelGotTriggered", true);
                linkedUI.GetComponent<Slider>().value = .83f;
            }
            else
            {
                // geht dann hoch abhängig von timeToCompleteComponents (im Inspektor von der Challenge gesetzt)
                linkedUI.transform.GetChild(0).GetComponent<Animator>().speed = Mathf.Lerp(1f, .2f, ExtensionMethods.Remap(timer, 0, timeToCompleteComponents, 0, 1));
                linkedUI.GetComponent<Slider>().value = Mathf.Lerp(.83f, .49f, ExtensionMethods.Remap(timer, 0, timeToCompleteComponents, 0, 1));
            }
        }
    }

    public void ToggleInvertedSliding()
    {
        // get value after value was changed in the checkbox
        var value = invertedSlidingToggle.isOn;
        if (value)
        {
            PlayerPrefs.SetInt("UseInvertedSliding", 1);
            ObjectManager.instance.pSM.stats.useInvertedSliding = value;
        }
        else
        {
            PlayerPrefs.SetInt("UseInvertedSliding", 0);
            ObjectManager.instance.pSM.stats.useInvertedSliding = value;
        }
    }

    public void ToggleJumpForLadderPush()
    {
        var value = jumpForLadderPushToggle.isOn;
        if (value)
        {
            PlayerPrefs.SetInt("UseJumpForLadderPush", 1);
            ObjectManager.instance.pSM.stats.useJumpForLadderPush = value;
        }
        else
        {
            PlayerPrefs.SetInt("UseJumpForLadderPush", 0);
            ObjectManager.instance.pSM.stats.useJumpForLadderPush = value;
        }
    }
}