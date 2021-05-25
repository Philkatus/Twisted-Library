using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class VFX_Manager : MonoBehaviour
{
    [SerializeField] GameObject Cloud;
    [SerializeField] GameObject player;
    [SerializeField] DecalProjector projector;
    PlayerMovementStateMachine pSM;
    
    Vector3 offset;
    private void Start()
    {
        offset = transform.position - player.transform.position;
        pSM = player.GetComponent<PlayerMovementStateMachine>();
        projector = this.GetComponent<DecalProjector>();
        Cloud.SetActive(false);
    }
    void Update()
    {
        transform.position = player.transform.position + offset;
        if (pSM.playerState == PlayerMovementStateMachine.PlayerState.inTheAir || pSM.playerState == PlayerMovementStateMachine.PlayerState.walking)
        {
            projector.enabled = true;
        }
        else
        {
            projector.enabled = false;
        }
    }

    void PlayParticleEffect(GameObject particleGameObject)
    {
        particleGameObject.SetActive(true);
        particleGameObject.GetComponent<ParticleSystem>().Play();
    }

    public void OnStateChangedWalking()
    {
        Debug.Log("Land");
        PlayParticleEffect(Cloud);
    }
    //Instantiate all Effects
    //Set Effects on State-Changes
}
