using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using PathCreation;

public class VFX_Manager : MonoBehaviour
{
    Rail CurrentRail;
    public Rail currentRail
    {
        get { return CurrentRail; }
        set
        {
            CurrentRail = value;
            if (value == null)
            {
                DisableParticleEffect(snappingFeedback);
            }
            else if (!snappingFeedback.activeInHierarchy && pSM.playerState != PlayerMovementStateMachine.PlayerState.swinging)
            {
                snappingFeedback.SetActive(true);
            }

        }
    }

    [SerializeField] GameObject player, swingingFeedback, snappingFeedback;

    PlayerMovementStateMachine pSM;
    DecalProjector projector;
    GameObject cloud;
    Vector3 offset;
    private void Start()
    {

        // Set all Effects
        cloud = transform.GetChild(2).gameObject;
        snappingFeedback = transform.GetChild(1).gameObject;
        projector = transform.GetChild(0).GetComponent<DecalProjector>();



        offset = transform.position - player.transform.position;
        pSM = player.GetComponent<PlayerMovementStateMachine>();
        cloud.SetActive(false);

        //unparent the snapping Feedback
        snappingFeedback.transform.SetParent(pSM.transform.parent);
    }
    void Update()
    {
        transform.position = player.transform.position + offset;
        if (pSM.playerState == PlayerMovementStateMachine.PlayerState.inTheAir
            || pSM.playerState == PlayerMovementStateMachine.PlayerState.walking)
        {
            projector.enabled = true;
        }
        else
        {
            projector.enabled = false;
        }
        if (snappingFeedback.activeInHierarchy)
            MoveSnappingFeedback();
    }
    #region OnStateChanged
    public void OnStateChangedWalking(bool land)
    {
        PlayParticleEffect(snappingFeedback);
        projector.gameObject.SetActive(true);
        if (land)
        {
            PlayParticleEffect(cloud);
        }

    }
    public void OnStateChangedInAir()
    {
        PlayParticleEffect(snappingFeedback);
        projector.gameObject.SetActive(true);
    }
    public void OnStateChangedSwinging()
    {
        DisableParticleEffect(snappingFeedback);
        projector.gameObject.SetActive(false);
    }
    #endregion
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
    void MoveSnappingFeedback()
    {
        if (currentRail != null)
            snappingFeedback.transform.position = currentRail.pathCreator.path.GetClosestPointOnPath(transform.position);
    }

}
