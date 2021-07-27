using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeScripts : MonoBehaviour
{
    private GameObject player;

    private ValuesScriptableObject stats;

    public Upgrade upgrade;

    UILogic uILogic;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stats = player.GetComponentInChildren<PlayerMovementStateMachine>().stats;
#if UNITY_EDITOR
        Cursor.visible = true;
        if (!RailSearchManager.instance.useAllSkils)
        {
            stats.canLadderFold = false;
            stats.canLadderPush = false;
            stats.canSlide = false;
        }
        else
        {
            stats.canLadderFold = true;
            stats.canLadderPush = true;
            stats.canSlide = true;
        }
#endif
        uILogic = ObjectManager.instance.uILogic;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            ObjectManager.instance.animationStateController.CollectUpgrade();
            if (upgrade == Upgrade.canLadderFold)
            {
                stats.canLadderFold = true;
                ObjectManager.instance.pSM.ladderSizeStateMachine.upGrades[0].enabled = true;
                uILogic.StartCoroutine(uILogic.ShowAndHideCatapultExplanation());
            }
            else if (upgrade == Upgrade.canLadderPush)
            {
                stats.canLadderPush = true;
                ObjectManager.instance.pSM.ladderSizeStateMachine.upGrades[1].enabled = true;
                uILogic.StartCoroutine(uILogic.ShowAndHideLadderPushExplanation());
            }
            else if (upgrade == Upgrade.canSlide)
            {
                stats.canSlide = true;
                ObjectManager.instance.pSM.ladderSizeStateMachine.upGrades[2].enabled = true;
                uILogic.StartCoroutine(uILogic.ShowAndHideSlidingExplanation());
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
