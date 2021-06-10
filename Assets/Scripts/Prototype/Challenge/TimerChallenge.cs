using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TimerChallenge : MonoBehaviour
{

    public float timeForChallenge;
    private float time;
    public List<GameObject> boxesToGet = new List<GameObject>();
    public int nbrOfBoxesCollected;
    public bool isChallengeActive, isCompleted;

    public GameObject challengePanel;
    public Text UITimer, UICollectedInfo, prefabText;

    public Material unactive, active;
    public bool enablesSomething;
    public Animator animator;

    [SerializeField] private Transform player, respawnPoint;
    public InputActionAsset actionAsset;
    InputActionMap playerControlsMap;
    InputAction stopChallengeAction;
    InputAction restartButton;

    // Start is called before the first frame update
    void Start()
    {
        nbrOfBoxesCollected = 0;
        time = timeForChallenge;

        UpdateUI();

        playerControlsMap = actionAsset.FindActionMap("PlayerControls");
        playerControlsMap.Enable();
        stopChallengeAction = playerControlsMap.FindAction("StopChallenge");
        restartButton = playerControlsMap.FindAction("Restart");

        prefabText.text = "Pick Up to start Challenge";
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBoxes();

        UpdateUI();

        if (isChallengeActive)
        {
            time -= Time.deltaTime;
        }

        if (time <= 0)
        {
            //lose
            //challengePanel.SetActive(false);
            //isChallengeActive = false;
            //prefabText.text = "Pick Up to start Challenge";
            //UITimer.text = "Over!";

            //restart
            challengePanel.SetActive(false);
            isChallengeActive = false;
            time = timeForChallenge;
            prefabText.text = "Pick Up to start Challenge";

            foreach (GameObject box in boxesToGet)
            {
                box.SetActive(true);
                box.GetComponent<MeshRenderer>().enabled = false;
                //box.GetComponent<MeshRenderer>().material = unactive;
            }

            player.GetComponentInChildren<CharacterController>().enabled = false;
            player.transform.position = respawnPoint.transform.position;
            player.GetComponentInChildren<CharacterController>().enabled = true;
            Debug.Log("Lose");
        }

        if (nbrOfBoxesCollected == boxesToGet.Count)
        {
            //win
            challengePanel.SetActive(false);
            isChallengeActive = false;
            isCompleted = true;
            this.transform.parent.gameObject.SetActive(false);
            Debug.Log("Win");

            if (enablesSomething)
            {
                animator.enabled = true;
            }

        }

        //STOP
        if (stopChallengeAction.triggered)
        {
            Debug.Log("Stopped");
            challengePanel.SetActive(false);
            isChallengeActive = false;
            time = timeForChallenge;
            prefabText.text = "Pick Up to start Challenge";

            foreach (GameObject box in boxesToGet)
            {
                box.SetActive(true);
                box.GetComponent<MeshRenderer>().enabled = false;
                //box.GetComponent<MeshRenderer>().material = unactive;
            }
        }

        //RESTART
        if (isChallengeActive && restartButton.triggered)
        {
            Debug.Log("restart");
            challengePanel.SetActive(false);
            isChallengeActive = false;
            time = timeForChallenge;
            prefabText.text = "Pick Up to start Challenge";

            foreach (GameObject box in boxesToGet)
            {
                box.SetActive(true);
                box.GetComponent<MeshRenderer>().enabled = false;
                //box.GetComponent<MeshRenderer>().material = unactive;
            }

            player.GetComponentInChildren<CharacterController>().enabled = false;
            player.transform.position = respawnPoint.transform.position;
            player.GetComponentInChildren<CharacterController>().enabled = true;
        }

    }

    public void UpdateBoxes()
    {
        nbrOfBoxesCollected = 0;

        foreach (GameObject box in boxesToGet)
        {
            if (!box.activeSelf)
            {
                nbrOfBoxesCollected++;
            }

            box.GetComponent<TimerChallengeBox>().isMyChallengeActive = isChallengeActive;
        }
    }

    public void UpdateUI()
    {
        if (isChallengeActive)
        {
            UITimer.text = time.ToString();
            UICollectedInfo.text = nbrOfBoxesCollected.ToString() + "/" + boxesToGet.Count.ToString() + " boxes";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            foreach (GameObject box in boxesToGet)
            {
                box.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isChallengeActive)
        {
            foreach (GameObject box in boxesToGet)
            {
                box.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }
}
