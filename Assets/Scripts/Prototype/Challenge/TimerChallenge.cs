using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerChallenge : MonoBehaviour
{

    public float timeForChallenge;
    private float time;
    public List<GameObject> boxesToGet;
    public int nbrOfBoxesCollected;
    public bool isChallengeActive;

    public GameObject challengePanel;
    public Text UITimer, UICollectedInfo, prefabText;

    public Material unactive, active;

    // Start is called before the first frame update
    void Start()
    {
        nbrOfBoxesCollected = 0;
        time = timeForChallenge;

        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateBoxes();

        UpdateUI();

        time -= Time.deltaTime;

        if(time <= 0)
        {
            //lose
            challengePanel.SetActive(false);
            isChallengeActive = false;
            Debug.Log("Lose");
        }

        if(nbrOfBoxesCollected == boxesToGet.Count)
        {
            //win
            challengePanel.SetActive(false);
            isChallengeActive = false;
            Debug.Log("Win");
        }

    }

    public void UpdateBoxes()
    {
        nbrOfBoxesCollected = 0;

        foreach (GameObject box in boxesToGet)
        {
            if (!box.GetComponent<MeshRenderer>().enabled)
            {
                nbrOfBoxesCollected++;
            }
            
            box.GetComponent<TimerChallengeBox>().isMyChallengeActive = isChallengeActive;
        }
    }

    public void UpdateUI()
    {
        UITimer.text = time.ToString();
        UICollectedInfo.text = nbrOfBoxesCollected.ToString() + "/" + boxesToGet.Count.ToString() + " boxes";
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            foreach(GameObject box in boxesToGet)
            {
                box.GetComponent<MeshRenderer>().material = active;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isChallengeActive)
        {
            foreach (GameObject box in boxesToGet)
            {
                box.GetComponent<MeshRenderer>().material = unactive;
            }
        }
    }
}
