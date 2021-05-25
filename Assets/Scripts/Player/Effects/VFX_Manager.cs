using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX_Manager : MonoBehaviour
{
    [SerializeField] GameObject Cloud;
    // Start is called before the first frame update
    void Start()
    {
        Cloud.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void PlayParticleEffect(GameObject particleGameObject)
    {
        particleGameObject.SetActive(true);
        particleGameObject.GetComponent<ParticleSystem>().Play();
    }

    public void OnStageChangedWalking()
    {
        PlayParticleEffect(Cloud);
    }
    //Instantiate all Effects
    //Set Effects on State-Changes
}
