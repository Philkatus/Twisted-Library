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
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            if(upgrade == Upgrade.canLadderFold)
            {
                stats.canLadderFold = true;
            }
            else if(upgrade == Upgrade.canLadderPush)
            {
                stats.canLadderPush = true;
            }
            else if (upgrade == Upgrade.canSlide)
            {
                stats.canSlide = true;
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
