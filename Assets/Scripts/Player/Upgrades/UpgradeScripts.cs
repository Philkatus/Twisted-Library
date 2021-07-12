using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeScripts : MonoBehaviour
{
    private GameObject player;

    private ValuesScriptableObject stats;

    public Upgrade upgrade;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stats = player.GetComponentInChildren<PlayerMovementStateMachine>().stats;
        stats.canLadderFold = false;
        stats.canLadderPush = false;
        stats.canSlide = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (upgrade == Upgrade.canLadderFold)
            {
                stats.canLadderFold = true;
                ObjectManager.instance.pSM.ladderSizeStateMachine.upGrades[0].enabled = true;
            }
            else if (upgrade == Upgrade.canLadderPush)
            {
                stats.canLadderPush = true;
                ObjectManager.instance.pSM.ladderSizeStateMachine.upGrades[1].enabled = true;
            }
            else if (upgrade == Upgrade.canSlide)
            {
                stats.canSlide = true;
                ObjectManager.instance.pSM.ladderSizeStateMachine.upGrades[2].enabled = true;
            }

            Destroy(this.gameObject);
        }
    }
}

public enum Upgrade
{
    canLadderPush,
    canLadderFold,
    canSlide
}
