using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class VFX_Manager : MonoBehaviour
{
    [SerializeField] GameObject effectParent;
    [SerializeField] GameObject player;
    [SerializeField] GameObject swingingFeedback;

    PlayerMovementStateMachine pSM;
    DecalProjector projector;
    GameObject snappingFeedback;
    GameObject cloud;
    Vector3 offset;
    private void Start()
    {

        // Set all Effects
        cloud = effectParent.transform.GetChild(2).gameObject;
        snappingFeedback = effectParent.transform.GetChild(1).gameObject;
        projector = effectParent.transform.GetChild(0).GetComponent<DecalProjector>();

        offset = transform.position - player.transform.position;
        pSM = player.GetComponent<PlayerMovementStateMachine>();
        cloud.SetActive(false);
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
    void DisableParticleEffect(GameObject particleGameObject)
    {
        particleGameObject.GetComponent<ParticleSystem>().Stop();
        particleGameObject.SetActive(false);
    }

    public void PlaceSwingingFeedback()
    {

    }
    public void MoveSnappingFeedback()
    {

    }
    #region OnStateChanged
    public void OnStateChangedWalking()
    {
        PlayParticleEffect(cloud);
    }
    public void OnStateChangedInAir()
    {
        
    }
    public void OnStateChangedSwinging()
    {
        
    }
    #endregion
}
