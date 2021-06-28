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
    [SerializeField] GameObject player, swingingFeedback, sparkleBurstL, sparkleBurstR, speedLinesS;
    [SerializeField] VisualEffect ladderPushLeft, ladderPushRight;

    PlayerMovementStateMachine pSM;
    DecalProjector projector;
    GameObject cloud, snappingFeedback;
    Vector3 offset;

    bool smokeOn = false;
    float smokeTimer = .5f;

    VisualEffect sparkleBurstLeft, sparkleBurstRight, speedLinesSliding;
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

        //set Sliding Speedlines
        speedLinesSliding = speedLinesS.GetComponent<VisualEffect>();
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
                smokeTimer = .5f;
            }
        }

        if (weAreSliding)
        {
            if (pSM.slidingInput <= -1 && pSM.currentSlidingSpeed > 0)//Links
            {
                sparkleBurstL.transform.rotation = Quaternion.Euler(212, 287, 85);
                sparkleBurstR.transform.rotation = Quaternion.Euler(212, 287, 85);
                CalculateSlideSpeedLineRotation(-220f, -180, 0);
                //speedLinesS.transform.rotation = Quaternion.Euler(speedLinesS.transform.rotation.x, -230f, speedLinesS.transform.rotation.z);
            }
            if (pSM.slidingInput >= 1 && pSM.currentSlidingSpeed > 0)//Rechts
            {
                sparkleBurstL.transform.rotation = Quaternion.Euler(211, 462, -85);
                sparkleBurstR.transform.rotation = Quaternion.Euler(211, 462, -85);
                CalculateSlideSpeedLineRotation(-140, -180, 0);
                //speedLinesS.transform.rotation = Quaternion.Euler(speedLinesS.transform.rotation.x, -140f, speedLinesS.transform.rotation.z);
            }

            SlidingSparkleIntensity(sparkleBurstLeft);
            SlidingSparkleIntensity(sparkleBurstRight);
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
            speedLinesSliding.SetFloat("_SpeedIntensity", 0);
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
            speedLinesSliding.SetFloat("_SpeedIntensity", 0);
        }
        if (pSM.currentSlidingSpeed <= pSM.stats.maxSlidingSpeed * .9)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(0, 0));
            vfx.SetInt("_FlameIntensity", 0);
            speedLinesSliding.SetFloat("_SpeedIntensity", 0);
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
        }*/
        if (pSM.currentSlidingSpeed <= pSM.stats.maxSlidingSpeed * .7 && pSM.currentSlidingSpeed > pSM.stats.maxSlidingSpeed * .5)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(0.01f, .1f));
            vfx.SetInt("_FlameIntensity", 1);
            speedLinesSliding.SetFloat("_SpeedIntensity", 10);
        }
        if (pSM.currentSlidingSpeed <= pSM.stats.maxSlidingSpeed * .9 && pSM.currentSlidingSpeed > pSM.stats.maxSlidingSpeed * .7)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(.1f, 1));
            vfx.SetInt("_FlameIntensity", 2);
            speedLinesSliding.SetFloat("_SpeedIntensity", 50);
        }
        if (pSM.currentSlidingSpeed >= pSM.stats.maxSlidingSpeed && pSM.currentSlidingSpeed > pSM.stats.maxSlidingSpeed * .9)
        {
            vfx.SetVector2("_SparkleSpawnCount", new Vector2(2, 5));
            vfx.SetInt("_FlameIntensity", 5);
            speedLinesSliding.SetFloat("_SpeedIntensity", 100);
        }       
    }
    void StopSlidingSparkle(VisualEffect vfx)
    {
        vfx.SendEvent("_StopBurst");
        speedLinesSliding.SetFloat("_SpeedIntensity", 10);
        weAreSliding = false;
    }
    void CalculateSlideSpeedLineRotation(float yRotationOrthogonal, float yRotationParallel, float yRotationEntgegengesetzt)
    {
        Vector3 directon = pSM.closestRail.pathCreator.path.GetDirectionAtDistance(pSM.currentDistance);

        if(Vector3.Dot(directon * pSM.slidingInput, Camera.main.transform.forward) > .9f)
        {
            speedLinesS.transform.rotation = Quaternion.Euler(speedLinesS.transform.rotation.x, yRotationParallel, speedLinesS.transform.rotation.z);
        }
        /*if (Vector3.Dot(directon * pSM.slidingInput, Camera.main.transform.forward) >= 0f)
        {
            speedLinesS.transform.rotation = Quaternion.Euler(speedLinesS.transform.rotation.x, yRotationOrthogonal, speedLinesS.transform.rotation.z);
        }*/
        if (Vector3.Dot(directon * pSM.slidingInput, Camera.main.transform.forward) < -.9f)
        {
            speedLinesS.transform.rotation = Quaternion.Euler(speedLinesS.transform.rotation.x, yRotationEntgegengesetzt, speedLinesS.transform.rotation.z);
        }
        else
        {
            speedLinesS.transform.rotation = Quaternion.Euler(speedLinesS.transform.rotation.x, yRotationOrthogonal, speedLinesS.transform.rotation.z);
        }
    }
}
