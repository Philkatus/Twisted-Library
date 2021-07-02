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

    InputActionAsset iaa;
    InputActionMap playerControlsMap;
    InputActionMap UIControlsMap;
    InputAction escape;
    InputAction escapeUI;
    public GameObject options, inGameUI;
    public GameObject controller;
    public GameObject keyboard;
    bool controlsActive = false;

    [SerializeField] Camera startCamera;
    [SerializeField] Camera playCamera;

    [SerializeField] GameObject startCanvas;
    public List<GameObject> uiElements;
    public List<Button> startCanvasButtons;
    bool startGotPressed = false;
    float timer = 0;

    public GameObject[] dummyTexts;
    bool optionGotSelectet = false;
    bool optionGotDeselectet = false;
    bool startcanvasDisabled = false;

    [SerializeField] List<Image> schalterUI;
    [SerializeField] List<Sprite> schalterImages;
    [SerializeField] List<Sprite> schalterImagesSide;
    Vector3 velocity = Vector3.zero;
    float schalterTimer = 5f;
    float timeCount = 2f;

    public GameObject handle;
    public Vector3 inactiveSize;
    public Vector3 activeSize;


    private void Start()
    {
        ObjectManager.instance.uILogic = this;
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
            optionGotSelectet = true;
        }
    }

    public void debugMessage()
    {
        Debug.LogError("options button");
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

    public void OnChallengeFailed(GameObject linkedUI, string type)
    {
        // verstecke wieder alle switches und zahnräder, weil die challenge gefailt wurde
        if (type == "switch")
        {
            linkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(activeSize, inactiveSize, timer);
            linkedUI.GetComponent<Slider>().value = .75f;
            linkedUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Slider>().value = .75f;
        }
        if (type == "cogwheel")
        {
            linkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(activeSize, inactiveSize, timer);
            linkedUI.transform.GetChild(0).GetComponent<Animator>().SetBool("WheelGotTriggered", false);
            linkedUI.transform.GetChild(0).GetComponent<Animator>().speed = 1f;
            linkedUI.GetComponent<Slider>().value = .49f;
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
            linkedUI.transform.GetChild(0).GetComponent<Animator>().SetBool("WheelGotTriggered", true);
            linkedUI.GetComponent<Slider>().value = .83f;
        }
    }

    public void OnChallengeCompleteLandmark(GameObject linkedUI)
    {
        // wenn alle switches an sind, wird das aufgerufen (challenge complete), wird im landmark script aufgerufen für das dazu
        // gehörige element vom landmark, was nun leuchten soll
        var image = linkedUI.GetComponent<Image>();
        image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);//Alpha
    }

    public void OnChallengeCompleteComponent(GameObject linkedUI, string type)
    {
        // hide component ui after challenge was completed

        if (type == "switch")
        {
            linkedUI.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().CrossFadeAlpha(0, .5f, false);
            linkedUI.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().CrossFadeAlpha(0, .5f, false);
            linkedUI.transform.GetChild(1).GetComponent<Image>().CrossFadeAlpha(0, .5f, false);
            linkedUI.transform.GetChild(1).transform.GetChild(0).GetComponent<Image>().CrossFadeAlpha(0, .5f, false);
        }
        if (type == "cogwheel")
        {
            linkedUI.transform.GetChild(0).GetComponent<Image>().CrossFadeAlpha(0, .5f, false);
            linkedUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().CrossFadeAlpha(0, .5f, false);
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
            linkedUI.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().enabled = true;
            linkedUI.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().enabled = true;
            linkedUI.transform.GetChild(1).GetComponent<Image>().enabled = true;
            linkedUI.transform.GetChild(1).transform.GetChild(0).GetComponent<Image>().enabled = true;

            linkedUI.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().CrossFadeAlpha(.7f, .2f, false);
            linkedUI.transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().CrossFadeAlpha(1f, .2f, false);
            linkedUI.transform.GetChild(1).GetComponent<Image>().CrossFadeAlpha(.7f, .2f, false);
            linkedUI.transform.GetChild(1).transform.GetChild(0).GetComponent<Image>().CrossFadeAlpha(1f, .2f, false);
        }
        if (type == "cogwheel")
        {

            linkedUI.GetComponent<RectTransform>().localScale = inactiveSize;
            linkedUI.GetComponent<Slider>().value = .49f;

            linkedUI.transform.GetChild(0).GetComponent<Image>().enabled = true;
            linkedUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().enabled = true;

            linkedUI.transform.GetChild(0).GetComponent<Image>().CrossFadeAlpha(.7f, .2f, false);
            linkedUI.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().CrossFadeAlpha(1f, .2f, false);
        }
    }

    public void OnChallengeStartedLandmark(GameObject firstLinkedUI, GameObject secondLinkedUI, GameObject thirdLinkedUI, GameObject groundUI, bool currentLandmark)
    {
        // parameter sind alle ui elemente vom landmark, die funktion wird zwei mal aufgerufen, einmal fur das Landmark, was gerade bespielt wird (bool currentLandmark ist true)
        // und dann für das andere Landmaek (bool currentLandmark ist false)

        if (currentLandmark)
        {
            // hebe es hervor
            groundUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(inactiveSize, activeSize, timer);
            firstLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(inactiveSize, activeSize, timer);
            secondLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(inactiveSize, activeSize, timer);
            thirdLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(inactiveSize, activeSize, timer);
        }
        else
        {
            // zeig es nur an
            groundUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(activeSize, inactiveSize, timer);
            firstLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(activeSize, inactiveSize, timer);
            secondLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(activeSize, inactiveSize, timer);
            thirdLinkedUI.GetComponent<RectTransform>().localScale = Vector3.Lerp(activeSize, inactiveSize, timer);
        }
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
                Debug.Log("This is my size:" + linkedUI.GetComponent<RectTransform>().localScale);
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
        var value = true;
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

    public void ToggleInvertedCameraAxis()
    {
        var value = true;
        if (value)
        {
            PlayerPrefs.SetInt("UseInvertedCamera", 1);
            // TO DO: invert Camera

        }
        else
        {
            PlayerPrefs.SetInt("UseInvertedCamera", 0);
            // TO DO: invert Camera
        }
    }

    public void ToggleJumpForLadderPush()
    {
        var value = true;
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