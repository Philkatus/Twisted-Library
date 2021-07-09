using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
            else if (railMats[0].GetFloat("_Multiplicator") == noIntensity && pSM.playerState != PlayerMovementStateMachine.PlayerState.swinging)
            {
                StartCoroutine(ReLightRail());
            }

        }
    }
    bool CanSwing = false;
    public bool canSwing
    {
        get { return CanSwing; }
        set
        {
            if (CanSwing != value)
            {
                if (value)
                {
                    StartCoroutine(ReLightRail());
                }

                else if (!value)
                {
                    StartCoroutine(FadeOutRail());
                }

            }
            CanSwing = value;
        }
    }
    #endregion
    #region PRIVATE
    [SerializeField] GameObject player, sparkleBurstL, sparkleBurstR, speedLinesS;
    [SerializeField] VisualEffect ladderPushLeft, ladderPushRight;
    [SerializeField] Material[] railMats;
    [Header("Snapping Light Up")]
    [SerializeField] float lightUpTime;
    [SerializeField] float fadeTime, normalWidth, broadWidth, normalGD, broadGD;
    [SerializeField] int noIntensity, normalIntensity, lightUpIntensity;
    PlayerMovementStateMachine pSM;

    GameObject cloud;
    Vector3 offset;

    bool smokeOn = false;
    float smokeTimer = .5f;

    VisualEffect sparkleBurstLeft, sparkleBurstRight, speedLinesSliding;
    bool weAreSliding = false;
    bool inStage = false;

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
        if (currentRail != null)
        {
            StartCoroutine(FadeOutRail());
        }

        if (land)
        {
            PlayParticleEffect(cloud);
            speedLinesSliding.SetFloat("_SpeedIntensity", 0);
        }

    }
    public void OnStateChangedInAir()
    {
        if (currentRail != null)
        {
            StartCoroutine(FadeOutRail());
        }
    }
    public void OnStateChangedSwinging()
    {
        StartCoroutine(LightRailUp());
    }
    public void OnResnap()
    {
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
        if (PlayerMovementStateMachine.PlayerState.swinging == pSM.playerState && pSM.lastRail != null)
        {
            Vector3 snappingPoint = currentRail.pathCreator.path.GetClosestPointOnPath(transform.position);
            snappingPoint = pSM.lastRail.pathCreator.path.GetClosestPointOnPath(transform.position);
            SetProperty(railMats, "_SnappingPoint", snappingPoint);
        }
        else if (pSM.closestRail != null)
        {
            Vector3 snappingPoint = currentRail.pathCreator.path.GetClosestPointOnPath(transform.position);
            snappingPoint = pSM.closestRail.pathCreator.path.GetClosestPointOnPath(transform.position);
            SetProperty(railMats, "_SnappingPoint", snappingPoint);
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
        SetProperty(railMats, "_SnappingPoint", Vector3.zero);
    }
    void SetProperty(Material[] railMats, string propertyName, Vector3 value)
    {
        foreach (Material railMat in railMats)
            railMat.SetVector(propertyName, value);
    }
    void SetProperty(Material[] railMats, string propertyName, float value)
    {
        foreach (Material railMat in railMats)
            railMat.SetFloat(propertyName, value);
    }
    #region ON TRIGGER
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Cogwheel")
        {
            VisualEffect vE = other.GetComponentInChildren<VisualEffect>();
            vE.SetVector3("currentSpeed", pSM.playerVelocity.normalized);
            vE.SendEvent("_Start");
        }
    }
    #endregion
    #region LIGHT RAIL UP
    IEnumerator LightRailUp()
    {
        inStage = true;
        StartCoroutine(LightUp(normalIntensity, lightUpIntensity, normalWidth, broadWidth, normalGD, broadGD, lightUpTime));
        yield return new WaitForSeconds(lightUpTime);
        //possibly play a particle effect here
        StartCoroutine(LightUp(lightUpIntensity, noIntensity, broadWidth, normalWidth, broadGD, normalGD, lightUpTime / 2));
        inStage = false;
    }
    IEnumerator ReLightRail()
    {
        if (!inStage)
        {
            inStage = true;
            StartCoroutine(LightUp(noIntensity, normalIntensity, normalWidth, normalWidth, normalGD, normalGD, lightUpTime));
            yield return new WaitForSeconds(fadeTime);
            inStage = false;
        }
    }
    IEnumerator FadeOutRail()
    {
        inStage = true;
        StartCoroutine(LightUp(normalIntensity, noIntensity, normalWidth, normalWidth, normalGD, normalGD, lightUpTime / 2));
        yield return new WaitForSeconds(lightUpTime);
        SetProperty(railMats, "_SnappingPoint", Vector3.zero);
        inStage = false;
    }
    private IEnumerator LightUp(float fromIntensity, float toIntensity, float fromWidth, float toWidth, float fromWidth2, float toWidth2, float time)
    {
        float timer = 0;
        while (timer < time)
        {
            float t = timer / time;
            float intensityValue = Mathf.Lerp(fromIntensity, toIntensity, t);
            float widthValue = Mathf.Lerp(fromWidth, toWidth, t);
            float widthValue2 = Mathf.Lerp(fromWidth2, toWidth2, t);
            SetProperty(railMats, "_Multiplicator", intensityValue);
            SetProperty(railMats, "_VD", widthValue);
            SetProperty(railMats, "_GD", widthValue2);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        SetProperty(railMats, "_Multiplicator", toIntensity);
        SetProperty(railMats, "_VD", toWidth);
        SetProperty(railMats, "_GD", toWidth2);
    }
    #endregion
}
