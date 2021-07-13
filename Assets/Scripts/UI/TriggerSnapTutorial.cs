using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSnapTutorial : MonoBehaviour
{
    public enum TriggerType
    {
        Snap, LetGo, Swing
    }

    public TriggerType triggerTutorialType;

    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            var type = "";
            if (triggerTutorialType == TriggerType.Snap)
            {
                type = "snap";
            }
            else if (triggerTutorialType == TriggerType.LetGo)
            {
                type = "letgo";
            }
            else
            {
                type = "swing";
            }
            ObjectManager.instance.uILogic.StartCoroutine(ObjectManager.instance.uILogic.ShowTutorialExplanation(type));
            Destroy(this.gameObject);
        }
    }
}
