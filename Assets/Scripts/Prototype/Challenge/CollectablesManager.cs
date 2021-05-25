using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectablesManager : MonoBehaviour
{
    public List<GameObject> collectables = new List<GameObject>();
    public Text UICollectablesInfo;
    public int nbrOfCollectedCollectables;


    private void Start()
    {
        if (collectables.Count == 0)
        {
            this.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        nbrOfCollectedCollectables = 0;
        foreach (GameObject collectable in collectables)
        {
            if (collectable == null)
            {
                nbrOfCollectedCollectables++;
            }
        }

        UICollectablesInfo.text = nbrOfCollectedCollectables.ToString() + "/" + collectables.Count.ToString() + " collectables collected";
    }
}
