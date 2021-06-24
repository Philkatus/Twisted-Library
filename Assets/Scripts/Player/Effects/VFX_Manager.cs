using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using PathCreation;
using UnityEngine.VFX;

public class VFX_Manager : MonoBehaviour
{
    #region GET/SET
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
    bool CanSwing = true;
    public bool canSwing
    {
        get { return CanSwing; }
        set
        {
            CanSwing = value;
            if (value && !swingingFeedback.activeInHierarchy)
                swingingFeedback.SetActive(true);
            else if (!value && swingingFeedback.activeInHierarchy)
                swingingFeedback.SetActive(false);
        }
    }
    #endregion
    #region PRIVATE
    [SerializeField] GameObject player, swingingFeedback, sparkleBurstL, sparkleBurstR;
    [SerializeField] VisualEffect ladderPushLeft, ladderPushRight;

    PlayerMovementStateMachine pSM;
    DecalProjector projector;
    GameObject cloud, snappingFeedback;
    Vector3 offset;

    bool smokeOn = false;
    float smokeTimer = 1f;

    VisualEffect sparkleBurstLeft, sparkleBurstRight;
    bool weAreSliding = false;
    #endregion
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

        //Set Burst Visual Effect
        sparkleBurstLeft = sparkleBurstL.GetComponent<VisualEffect>();
        sparkleBurstRight = sparkleBurstR.GetComponent<VisualEffect>();
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

        if (smokeOn)
        {
            smokeTimer -= Time.deltaTime;

            if (smokeTimer <= 0)
            {
                smokeOn = false;
                ladderPushLeft.SendEvent("_Stop");
                ladderPushRight.SendEvent("_Stop");
                smokeTimer = 1f;
            }
        }

        if (weAreSliding)
        {
            SlidingSparkleIntensity(sparkleBurstLeft);
            SlidingSparkleIntensity(sparkleBurstRight);

            /*if (pSM.slideLeftInput >= 0 && pSM.currentSlidingSpeed >= pSM.stats.maxSlidingSpeed * .7f)
            {
                sparkleBurstRight.SetVector2("_SparkleSpawnCount", new Vector2(2, 14));
                sparkleBurstRight.SetInt("_FlameIntensity", 2);
                Debug.Log("Left");
            }
            else if (pSM.slideRightInput >= 0 && pSM.currentSlidingSpeed >= pSM.stats.maxSlidingSpeed * .7f)
            {
                sparkleBurstLeft.SetVector2("_SparkleSpawnCount", new Vector2(2, 14));
                sparkleBurstLeft.SetInt("_FlameIntensity", 2);
                Debug.Log("RIGHT");
            }*/
        }
    }
    #region OnStateChanged
    public void OnStateChangedWalking(bool land)
    {
        DisableParticleEffect(swingingFeedback);
        PlayParticleEffect(snappingFeedback);
        projector.gameObject.SetActive(true);
        if (land)
        {
            PlayParticleEffect(cloud);
        }

    }
    public void OnStateChangedInAir()
    {
        DisableParticleEffect(swingingFeedback);
        PlayParticleEffect(snappingFeedback);
        projector.gameObject.SetActive(true);
    }
    public void OnStateChangedSwinging()
    {
        PlayParticleEffect(swingingFeedback);
        DisableParticleEffect(snappingFeedback);
        projector.gameObject.SetActive(false);
    }
    public void OnStateChangedLadderPush()
    {
        StartLadderPushVFX(ladderPushLeft);
        StartLadderPushVFX(ladderPushRight);
    }
    public void OnStateChangedSlide()
    {
        StartSlidingSparkle(sparkleBurstLeft);
        StartSlidingSparkle(sparkleBurstRight);
    }
    public void OnStateChangedSlideEnd()
    {
        StopSlidingSparkle(sparkleBurstLeft);
        StopSlidingSparkle(sparkleBurstRight);
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
    void StartLadderPushVFX(VisualEffect vfx)
    {
        vfx.SendEvent("_Start");
        smokeOn = true;
    }
    void StartSlidingSparkle(VisualEffect vfx)
    {
        vfx.SetVector2("_SparkleSpawnCount", new Vector2(0, 0));
        vfx.SetInt("_FlameIntensity", 0);
        vfx.SendEvent("_StartBurst");
        weAreSliding = true;
    }
    void SlidingSparkleIntensity(VisualEffect vfx)
    {
        if (pSM.currentSlidingSpeed <= 0)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(0, 0));
            vfx.SetInt("_FlameIntensity", 0);
        }
        if (pSM.currentSlidingSpeed <= pSM.stats.maxSlidingSpeed * .9)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(0, 0));
            vfx.SetInt("_FlameIntensity", 0);
        }
        /*if (pSM.currentSlidingSpeed <= pSM.stats.maxSlidingSpeed * .2 && pSM.currentSlidingSpeed > 0)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(0, 0));
            vfx.SetInt("_FlameIntensity", 1);
        }
        if (pSM.currentSlidingSpeed <= pSM.stats.maxSlidingSpeed * .5 && pSM.currentSlidingSpeed > pSM.stats.maxSlidingSpeed * .2)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(0, 2));
            vfx.SetInt("_FlameIntensity", 5);
        }
        if (pSM.currentSlidingSpeed <= pSM.stats.maxSlidingSpeed * .7 && pSM.currentSlidingSpeed > pSM.stats.maxSlidingSpeed * .5)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(0, 3));
            vfx.SetInt("_FlameIntensity", 1);
        }*/
        if (pSM.currentSlidingSpeed <= pSM.stats.maxSlidingSpeed * .9 && pSM.currentSlidingSpeed > pSM.stats.maxSlidingSpeed * .7)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(1, 4));
            vfx.SetInt("_FlameIntensity", 2);
        }
        if (pSM.currentSlidingSpeed >= pSM.stats.maxSlidingSpeed && pSM.currentSlidingSpeed > pSM.stats.maxSlidingSpeed * .9)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(2, 7));
            vfx.SetInt("_FlameIntensity", 3);
        }       
    }
    void StopSlidingSparkle(VisualEffect vfx)
    {
        vfx.SendEvent("_StopBurst");
        weAreSliding = false;
    }
}
