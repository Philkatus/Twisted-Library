using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeScripts : MonoBehaviour
{
    private GameObject player;

    private ValuesScriptableObject stats;
    private PlayerMovementStateMachine psm;

    public Upgrade upgrade;

    UILogic uILogic;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        stats = player.GetComponentInChildren<PlayerMovementStateMachine>().stats;
        psm = ObjectManager.instance.pSM;
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
                psm.ladderSizeStateMachine.upGrades[0].enabled = true;
                psm.effects.MakeUpgradeGlow(psm.ladderSizeStateMachine.upGrades[0]);
                uILogic.StartCoroutine(uILogic.ShowAndHideCatapultExplanation());
            }
            else if (upgrade == Upgrade.canLadderPush)
            {
                stats.canLadderPush = true;
                psm.ladderSizeStateMachine.upGrades[1].enabled = true;
                psm.effects.MakeUpgradeGlow(psm.ladderSizeStateMachine.upGrades[1]);
                uILogic.StartCoroutine(uILogic.ShowAndHideLadderPushExplanation());
            }
            else if (upgrade == Upgrade.canSlide)
            {
                stats.canSlide = true;
                psm.ladderSizeStateMachine.upGrades[2].enabled = true;
                psm.effects.MakeUpgradeGlow(psm.ladderSizeStateMachine.upGrades[2]);
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
