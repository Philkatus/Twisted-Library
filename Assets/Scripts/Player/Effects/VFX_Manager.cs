using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using PathCreation;
using UnityEngine.VFX;
using System.Threading;

public class VFX_Manager : MonoBehaviour
{
    #region PUBLIC
    public float lerpSpeed = .01f;
    #endregion
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
    bool OnWall = false;
    public bool onWall
    {
        get { return OnWall; }
        set
        {
            StartCoroutine(AnimateWall(wallTime));
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
    [SerializeField] Color[] normalColor, swingingColor;
    [Header("Decal Shadow")]
    [SerializeField] DecalProjector shadow;
    [SerializeField] VisualEffect landingBubbles;
    [SerializeField] AnimationCurve shadowSize, impactCurve, hardImpactCurve;
    [SerializeField] float shadowRemapMin, shadowRemapMax, decalScale, minJumpTime, maxJumpTime;
    [Header("Wall Projection")]
    [SerializeField] GameObject ladder;
    [SerializeField] DecalProjector wallProjector;
    [SerializeField] float wallTime;
    [Header("Water Steps")]
    [SerializeField] DecalProjector waterStepsLeft;
    [SerializeField] DecalProjector waterStepsRight;
    [SerializeField] float waterSpeed;
    [Header("Double Jump")]
    [SerializeField] DecalProjector doubleJump;
    [SerializeField] VisualEffect doubleJumpSpray, bigDoubleJumpSpray;

    PlayerMovementStateMachine pSM;

    Vector3 offset, lastPositionWall, sprayPosition;

    bool smokeOn = false;
    float smokeTimer = .5f, inAirTimer = 0, wallOffsetUp, wallOffsetBack;

    VisualEffect sparkleBurstLeft, sparkleBurstRight, speedLinesSliding;
    bool weAreSliding = false;
    bool inStage = false, inAir, wallProjecting;
    #endregion

    private void Start()
    {
        sprayPosition = bigDoubleJumpSpray.transform.localPosition;
        // Set all Effects
        offset = transform.GetChild(0).transform.position - player.transform.position;

        wallOffsetUp = wallProjector.transform.position.y - ladder.transform.position.y;
        wallOffsetBack = wallProjector.transform.position.z - ladder.transform.position.z;
        pSM = player.GetComponent<PlayerMovementStateMachine>();

        //Set Burst Visual Effect
        sparkleBurstLeft = sparkleBurstL.GetComponent<VisualEffect>();
        sparkleBurstRight = sparkleBurstR.GetComponent<VisualEffect>();

        //set Sliding Speedlines
        speedLinesSliding = speedLinesS.GetComponent<VisualEffect>();
    }

    void Update()
    {
        //offsets
        transform.GetChild(0).transform.position = player.transform.position + offset;
        if (wallProjecting)
        {
            wallProjector.transform.position = lastPositionWall;
        }


        MoveSnappingFeedback();
        if (PlayerMovementStateMachine.PlayerState.inTheAir == pSM.playerState)
        {
            UpdateShadowSize();
        }

        //Smoke
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

        //sliding
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
        inAir = false;
        SetProperty(railMats, "_EmissionColor", normalColor, fadeTime);
        if (currentRail != null)
        {
            StartCoroutine(FadeOutRail());
        }

        if (land)
        {
            speedLinesSliding.SetFloat("_SpeedIntensity", 0);
            UpdateShadowSize(true);
        }
    }

    public void OnStateChangedInAir()
    {
        inAir = true;
        StartCoroutine(InAirTime());
        SetProperty(railMats, "_EmissionColor", normalColor, fadeTime);
        if (currentRail != null)
        {
            StartCoroutine(FadeOutRail());
        }
    }

    public void OnStateChangedSwinging()
    {
        inAir = false;
        shadow.size = new Vector3(0, 0, shadowRemapMax);
        StartCoroutine(LightRailUp());
        SetProperty(railMats, "_EmissionColor", swingingColor, lightUpTime);

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

    #region UPDATE
    void MoveSnappingFeedback()
    {
        if (PlayerMovementStateMachine.PlayerState.swinging == pSM.playerState && pSM.lastRail != null)
        {
            Vector3 snappingPoint = pSM.lastRail.pathCreator.path.GetClosestPointOnPath(transform.GetChild(0).position);
            SetProperty(railMats, "_SnappingPoint", snappingPoint);
        }
        else if (pSM.closestRail != null)
        {
            Vector3 snappingPoint = pSM.closestRail.pathCreator.path.GetClosestPointOnPath(transform.GetChild(0).position);
            SetProperty(railMats, "_SnappingPoint", snappingPoint);
        }
        else
            StartCoroutine(FadeOutRail());
    }

    void UpdateShadowSize(bool end = false)
    {
        if (!end)
        {
            Vector3 groundPoint = Vector3.zero;
            RaycastHit[] hits = Physics.RaycastAll(transform.position, Vector3.down, shadowRemapMax);
            foreach (RaycastHit hit in hits)
            {
                if (hit.transform.gameObject.layer == 6)
                {
                    groundPoint = hit.point;
                    break;
                }
            }
            if (groundPoint != Vector3.zero)
            {
                float distance = Vector3.Distance(groundPoint, shadow.transform.position);
                distance = Mathf.Clamp(distance, shadowRemapMin, shadowRemapMax);
                distance = ExtensionMethods.Remap(distance, shadowRemapMin, shadowRemapMax, 0, 1);
                float curvepoint = shadowSize.Evaluate(distance);
                shadow.size = new Vector3(curvepoint * decalScale, curvepoint * decalScale, shadowRemapMax);
            }
        }
        else
        {
            StartCoroutine(OnImpact(inAirTimer));
        }
    }
    #endregion

    #region Namin's VFX
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
    #endregion

    private void OnApplicationQuit()
    {
        SetProperty(railMats, "_SnappingPoint", Vector3.zero);
        SetProperty(railMats, "_EmissionColor", normalColor, fadeTime);
        wallProjector.material.SetFloat("_WallTime", 0);
    }

    #region SET PROPERTY
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

    void SetProperty(Material[] railMats, string propertyName, Color[] value, float time)
    {
        for (int i = 0; i < railMats.Length; i++)
            StartCoroutine(ChangePropertyColor(railMats[i], propertyName, railMats[i].GetColor(propertyName), value[i], time));
    }

    IEnumerator ChangePropertyColor(Material mat, string propertyName, Color fromColor, Color toColor, float time)
    {
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        float timer = 0;
        while (timer < time)
        {
            float t = timer / time;
            Color intensityValue = Color.Lerp(fromColor, toColor, t);
            mat.SetColor(propertyName, intensityValue);
            timer += Time.deltaTime;
            yield return delay;
        }
        mat.SetColor(propertyName, toColor);
    }
    #endregion

    #region CHALLENGES
    public void PlayCogwheel(Transform parentObj)
    {
        VisualEffect vE = parentObj.GetComponentInChildren<VisualEffect>();
        vE.SetVector3("_CurrentSpeed", pSM.playerVelocity.normalized);
        vE.SendEvent("_Start");
    }
    #endregion

    #region SHADOW
    IEnumerator OnImpact(float inAirTime)
    {
        float jumpIntensity = Mathf.Clamp(inAirTime, minJumpTime, maxJumpTime);
        jumpIntensity = ExtensionMethods.Remap(jumpIntensity, minJumpTime, maxJumpTime, 0, 1);

        float timer = 0;
        float time = impactCurve.keys[impactCurve.length - 1].time;
        bool castEffect = false;
        while (timer < time)
        {
            float t = timer / time;

            float curvepoint = impactCurve.Evaluate(t) * decalScale;
            float curvepoint2 = hardImpactCurve.Evaluate(t) * decalScale;
            curvepoint = Mathf.Lerp(curvepoint, curvepoint2, jumpIntensity);

            shadow.size = new Vector3(curvepoint, curvepoint, shadowRemapMax);

            if (t >= 0.2f && !castEffect)
            {
                landingBubbles.SetFloat("_Radius", curvepoint);
                landingBubbles.SendEvent("_Start");
                castEffect = true;
            }

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        shadow.size = new Vector3(0, 0, shadowRemapMax);
    }
    IEnumerator InAirTime()
    {
        inAirTimer = 0;
        while (inAir)
        {
            inAirTimer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }
    #endregion

    #region DOUBLE JUMP
    public void PlayCoroutine(Vector3 planeNormal, Vector3 planeUp)
    {
        StartCoroutine(OnDoubleJump(inAirTimer, planeNormal, planeUp));
    }

    IEnumerator OnDoubleJump(float inAirTime, Vector3 planeNormal, Vector3 planeUp)
    {
        float sprayY = bigDoubleJumpSpray.transform.position.y;
        bigDoubleJumpSpray.transform.SetParent(this.transform);

        float jumpIntensity = Mathf.Clamp(inAirTime, minJumpTime, maxJumpTime);
        jumpIntensity = ExtensionMethods.Remap(jumpIntensity, minJumpTime, maxJumpTime, 0, 1);

        float timer = 0;
        float time = impactCurve.keys[impactCurve.length - 1].time;
        bool castEffect = false;
        while (timer < time)
        {
            doubleJump.transform.LookAt(doubleJump.transform.position-planeNormal);
            float t = timer / time;

            float curvepoint = impactCurve.Evaluate(t) * decalScale;
            float curvepoint2 = hardImpactCurve.Evaluate(t) * decalScale;
            curvepoint = Mathf.Lerp(curvepoint, curvepoint2, jumpIntensity);

            doubleJump.size = new Vector3(curvepoint, curvepoint, shadowRemapMax);
            bigDoubleJumpSpray.transform.position = new Vector3(doubleJumpSpray.transform.position.x, sprayY, doubleJumpSpray.transform.position.z);
            if (t >= 0.2f && !castEffect)
            {
                doubleJumpSpray.SetFloat("_Radius", curvepoint);
                doubleJumpSpray.SetVector3("_Normal", planeNormal);
                doubleJumpSpray.SetVector3("_Up", planeUp);
                doubleJumpSpray.SendEvent("_Start");

                bigDoubleJumpSpray.SetFloat("_Radius", curvepoint);
                bigDoubleJumpSpray.SetVector3("_Normal", planeNormal);
                bigDoubleJumpSpray.SetVector3("_Up", planeUp);
                bigDoubleJumpSpray.SendEvent("_Start");

                castEffect = true;
            }

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        doubleJump.size = new Vector3(0, 0, shadowRemapMax);
        bigDoubleJumpSpray.transform.SetParent(transform.GetChild(0));
        bigDoubleJumpSpray.transform.localPosition = sprayPosition;
    }
    #endregion

    #region WALL PROJECTION
    IEnumerator AnimateWall(float time)
    {
        yield return new WaitForEndOfFrame();
        wallProjecting = true;
        wallProjector.transform.position = pSM.ladder.transform.position + Vector3.up * wallOffsetUp + ladder.transform.forward * wallOffsetBack;
        lastPositionWall = pSM.ladder.transform.position + Vector3.up * wallOffsetUp + ladder.transform.forward * wallOffsetBack;
        wallProjector.transform.rotation = Quaternion.Euler(wallProjector.transform.eulerAngles.x, ladder.transform.eulerAngles.y, wallProjector.transform.eulerAngles.z);
        float timer = 0;
        while (timer < time)
        {
            if (pSM.playerState == PlayerMovementStateMachine.PlayerState.swinging)
                lastPositionWall = pSM.ladder.transform.position + Vector3.up * wallOffsetUp + ladder.transform.forward * wallOffsetBack;
            float t = timer / time;
            float currentTime = Mathf.Lerp(0, 1.22f, t);
            wallProjector.material.SetFloat("_WallTime", currentTime);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        wallProjector.material.SetFloat("_WallTime", 0);
        wallProjecting = false;
    }
    #endregion
    #region WATER STEPS
    IEnumerator ExtendWater()
    {
        float timer = 0;
        while (timer < waterSpeed)
        {
            float t = timer / waterSpeed;
            float currentSize = Mathf.Lerp(0, 0.3f, t);
            waterStepsLeft.size = new Vector3(currentSize, currentSize, 1);
            waterStepsRight.size = new Vector3(currentSize, currentSize, 1);
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        waterStepsLeft.size = new Vector3(0, 0, 1);
        waterStepsRight.size = new Vector3(0, 0, 1);
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
        WaitForEndOfFrame delay = new WaitForEndOfFrame();

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
            yield return delay;
        }
        SetProperty(railMats, "_Multiplicator", toIntensity);
        SetProperty(railMats, "_VD", toWidth);
        SetProperty(railMats, "_GD", toWidth2);
    }
    #endregion
}
