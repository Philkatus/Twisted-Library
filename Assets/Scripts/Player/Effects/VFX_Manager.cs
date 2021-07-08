using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using PathCreation;
using UnityEngine.VFX;
using System.Threading;

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
                StartCoroutine(FadeOutRail());
            }
            else if (railMat.GetFloat("_Multiplicator") == noIntensity && pSM.playerState != PlayerMovementStateMachine.PlayerState.swinging)
            {
                StartCoroutine(ReLightRail());
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
    [SerializeField] Material railMat;
    [Header("Snapping Light Up")]
    [SerializeField] float lightUpTime;
    [SerializeField] float fadeTime;
    [SerializeField] int noIntensity, normalIntensity, lightUpIntensity;
    PlayerMovementStateMachine pSM;

    GameObject cloud;
    Vector3 offset;

    bool smokeOn = false;
    float smokeTimer = .5f;

    VisualEffect sparkleBurstLeft, sparkleBurstRight, speedLinesSliding;
    bool weAreSliding = false;

    public float lerpSpeed = .01f;
    #endregion
    private void Start()
    {
        // Set all Effects
        cloud = transform.GetChild(0).gameObject;

        offset = transform.position - player.transform.position;
        pSM = player.GetComponent<PlayerMovementStateMachine>();
        cloud.SetActive(false);

        //Set Burst Visual Effect
        sparkleBurstLeft = sparkleBurstL.GetComponent<VisualEffect>();
        sparkleBurstRight = sparkleBurstR.GetComponent<VisualEffect>();

        //set Sliding Speedlines
        speedLinesSliding = speedLinesS.GetComponent<VisualEffect>();
    }
    void Update()
    {
        transform.position = player.transform.position + offset;

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
                CalculateSlideSpeedLineRotation();
            }
            if (pSM.slidingInput >= 1 && pSM.currentSlidingSpeed > 0)//Rechts
            {
                sparkleBurstL.transform.rotation = Quaternion.Euler(211, 462, -85);
                sparkleBurstR.transform.rotation = Quaternion.Euler(211, 462, -85);
                CalculateSlideSpeedLineRotation();
            }

            SlidingSparkleIntensity(sparkleBurstLeft);
            SlidingSparkleIntensity(sparkleBurstRight);
        }
    }
    #region OnStateChanged
    public void OnStateChangedWalking(bool land)
    {
        DisableParticleEffect(swingingFeedback);
        StartCoroutine(ReLightRail());

        if (land)
        {
            PlayParticleEffect(cloud);
            speedLinesSliding.SetFloat("_SpeedIntensity", 0);
        }

    }
    public void OnStateChangedInAir()
    {
        DisableParticleEffect(swingingFeedback);
        StartCoroutine(ReLightRail());

    }
    public void OnStateChangedSwinging()
    {
        PlayParticleEffect(swingingFeedback);
        StartCoroutine(LightRailUp());

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

    void MoveSnappingFeedback()
    {
        if (currentRail != null)
        {
            Vector3 snappingPoint = currentRail.pathCreator.path.GetClosestPointOnPath(transform.position);
            SetProperty(railMat, "_SnappingPoint", snappingPoint);
        }
        else
            StartCoroutine(FadeOutRail());
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
    void CalculateSlideSpeedLineRotation()
    {
        Vector3 directon = pSM.closestRail.pathCreator.path.GetDirectionAtDistance(pSM.currentDistance);

        if (Vector3.Dot(directon * pSM.slidingInput, Camera.main.transform.forward) >= 0f)
        {
            speedLinesS.transform.forward = Vector3.Lerp(speedLinesS.transform.forward, (directon * pSM.slidingInput) * -1f, lerpSpeed);
        }
        if (Vector3.Dot(directon * pSM.slidingInput, Camera.main.transform.forward) < -.75f)
        {
            speedLinesS.transform.forward = Vector3.Lerp(speedLinesS.transform.forward, Camera.main.transform.forward * -1, lerpSpeed);
        }
    }
    private void OnApplicationQuit()
    {
        SetProperty(railMat, "_SnappingPoint", Vector3.zero);
    }
    void SetProperty(Material mat, string propertyName, Vector3 value)
    {
        mat.SetVector(propertyName, value);
    }
    void SetProperty(Material mat, string propertyName, float value)
    {
        mat.SetFloat(propertyName, value);
    }

    #region LIGHT RAIL UP
    IEnumerator LightRailUp()
    {
        StartCoroutine(LightUp(normalIntensity, lightUpIntensity, lightUpTime));
        yield return new WaitForSeconds(lightUpTime);
        //possibly play a particle effect here
        StartCoroutine(LightUp(lightUpIntensity, noIntensity, fadeTime));
    }
    IEnumerator ReLightRail()
    {
        StartCoroutine(LightUp(noIntensity, lightUpIntensity, lightUpTime));
        yield return new WaitForSeconds(lightUpTime);
        StartCoroutine(LightUp(lightUpIntensity, normalIntensity, fadeTime));
    }
    IEnumerator FadeOutRail()
    {
        StartCoroutine(LightUp(normalIntensity, noIntensity, fadeTime));
        yield return new WaitForSeconds(lightUpTime);
        SetProperty(railMat, "_SnappingPoint", Vector3.zero);
    }
    private IEnumerator LightUp(float fromIntensity, float toIntensity, float time)
    {
        float timer = 0;
        while (timer < time)
        {
            float t = timer / time;
            float intensityValue = Mathf.Lerp(fromIntensity, toIntensity, t);
            SetProperty(railMat, "_Multiplicator", intensityValue);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SetProperty(railMat, "_Multiplicator", toIntensity);
    }
    #endregion
}
