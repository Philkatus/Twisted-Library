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
    [HideInInspector] public GameObject nextLandmark;
    #endregion
    #region GET/SET
    int RandomColor;
    int randomColor
    {
        get
        {
            int rc = RandomColor + 1;
            if (rc >= possibleColors.Length)
            {
                RandomColor = 0;
            }
            else
            {
                RandomColor = rc;
            }
            return RandomColor;
        }
        set
        {
            // this isnt called
        }
    }
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
    #region INSPECTOR
    [SerializeField] GameObject player, sparkleBurstL, sparkleBurstR;
    [SerializeField] bool useFadeOut, useNewWallProjector;
    [SerializeField] VisualEffect ladderPushLeft, ladderPushRight, upgradeCloud, stepLeft, stepRight;
    [SerializeField] Material[] railMats;
    [Header("Sliding")]
    [SerializeField] VisualEffect speedLinesSliding;
    [SerializeField] VisualEffect slidingTrail;
    [Header("Wheel Light Up")]
    [SerializeField] Material wheelMat;
    [SerializeField] float wheelIntensity = 5000000;
    [Header("Ansnap / LightUp")]
    [SerializeField] float lightUpTime;
    [SerializeField] float fadeTime, normalWidth, broadWidth, normalGD, broadGD;
    [SerializeField] int noIntensity, normalIntensity, lightUpIntensity;
    [SerializeField] Color[] normalColor, swingingColor;
    [SerializeField] VisualEffect snappingVFX;
    [Header("Decal Shadow")]
    [SerializeField] DecalProjector shadow;
    [SerializeField] VisualEffect landingBubbles;
    [SerializeField] AnimationCurve shadowSize, impactCurve, hardImpactCurve;
    [SerializeField] float shadowRemapMin, shadowRemapMax, decalScale, minJumpTime, maxJumpTime;
    [SerializeField] Color[] possibleColors;
    [SerializeField] float alpha = 130;
    [Header("Wall Projection")]
    [SerializeField] GameObject ladder;
    [SerializeField] DecalProjector wallProjector;
    [SerializeField] VisualEffect wallBubbles;
    [SerializeField] float wallTime;
    [Header("Water Steps")]
    [SerializeField] DecalProjector waterStepsLeft;
    [SerializeField] DecalProjector waterStepsRight;
    [SerializeField] float waterSpeed;
    [Header("Double Jump")]
    [SerializeField] DecalProjector doubleJump;
    [SerializeField] VisualEffect doubleJumpSpray, bigDoubleJumpSpray;
    [Header("Splash")]
    [SerializeField] VisualEffect splash;
    [Header("Evironment")]
    [SerializeField] float waterfallTime = 2;
    [SerializeField] VisualEffect wind;
    [SerializeField] GameObject windParent;
    #endregion

    #region PRIVATE
    PlayerMovementStateMachine pSM;

    Vector3 offset, lastPositionWall, sprayPosition;

    bool smokeOn = false, inWater = false, freshOutOfWater = false;
    float smokeTimer = .5f, inAirTimer = 0, wallOffsetUp, wallOffsetBack;

    VisualEffect sparkleBurstLeft, sparkleBurstRight;
    bool weAreSliding = false;
    bool inStage = false, inAir, wallProjecting;
    #endregion

    #region UNITY FUNCTIONS
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
    }

    void Update()
    {
        //offsets
        transform.GetChild(0).transform.position = player.transform.position + offset;
        transform.GetChild(0).transform.rotation = player.transform.rotation;



        if (wallProjecting)
        {
            wallProjector.transform.position = lastPositionWall;
            wallBubbles.transform.position = wallProjector.transform.position + wallProjector.transform.forward * -0.5f + Vector3.down * 0.5f;
        }
        Vector3 nextTrailPos = ladder.transform.position - ladder.transform.up * (pSM.ladderSizeStateMachine.ladderLength - 1f);
        slidingTrail.transform.position = Vector3.Lerp(slidingTrail.transform.position, nextTrailPos, 0.8f);
        slidingTrail.transform.LookAt(slidingTrail.transform.position - ladder.transform.forward, ladder.transform.up);
        wind.transform.parent.position = Vector3.Lerp(wind.transform.parent.position, windParent.transform.position, 0.8f);

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

        if (pSM.isOnWater)
        {
            waterStepsLeft.enabled = true;
            waterStepsRight.enabled = true;
        }
        else
        {
            waterStepsLeft.enabled = false;
            waterStepsRight.enabled = false;
        }
    }
    private void OnApplicationQuit()
    {
        SetProperty(railMats, "_SnappingPoint", Vector3.zero);
        SetProperty(railMats, "_EmissionColor", normalColor, fadeTime);
        wallProjector.material.SetFloat("_WallTime", 0);
    }
    #endregion

    #region OnStateChanged

    public void OnStateChangedWalking(bool land)
    {
        inAir = false;
        SetProperty(railMats, "_EmissionColor", normalColor, fadeTime);
        if (currentRail != null)
        {
            StartCoroutine(FadeOutRail());
        }

        if (land && !pSM.dismountedNoEffect)
        {
            speedLinesSliding.SetFloat("_SpeedIntensity", 0);
            UpdateShadowSize(true);
        }
        pSM.dismountedNoEffect = false;
        StopVFX("trail");
        StopVFX("speedlines");
        StopSlidingSparkle(sparkleBurstLeft);
        StopSlidingSparkle(sparkleBurstRight);
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
        StopVFX("trail");
        StopSlidingSparkle(sparkleBurstLeft);
        StopSlidingSparkle(sparkleBurstRight);
    }

    public void OnStateChangedSwinging()
    {
        inAir = false;
        shadow.size = new Vector3(0, 0, shadowRemapMax);
        StartCoroutine(LightRailUp());
        SetProperty(railMats, "_EmissionColor", swingingColor, lightUpTime);
        PlayVFX("snap");
        PlayVFX("speedlines");
        PlayVFX("trail");
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
        PlayVFX("trail");
        PlayVFX("speedlines");
    }
    public void OnStateChangedSlideEnd()
    {
        StopSlidingSparkle(sparkleBurstLeft);
        StopSlidingSparkle(sparkleBurstRight);
        StopVFX("trail");
        StopVFX("speedlines");
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
        if (!inWater)
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
        else
        {
            PlayVFX("splash");
        }
    }
    #endregion

    #region LADDER PUSH
    void StartLadderPushVFX(VisualEffect vfx)
    {
        vfx.SendEvent("_Start");
        smokeOn = true;
    }
    #endregion

    #region SLIDING
    // Namins Code
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
            speedLinesSliding.transform.forward = Vector3.Lerp(speedLinesSliding.transform.forward, (directon * pSM.slidingInput) * -1f, lerpSpeed);
        }
        if (Vector3.Dot(directon * pSM.slidingInput, Camera.main.transform.forward) < -.75f)
        {
            speedLinesSliding.transform.forward = Vector3.Lerp(speedLinesSliding.transform.forward, Camera.main.transform.forward * -1, lerpSpeed);
        }
    }
    #endregion

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
    Color GetColor(int i, bool fullOpacity = false, float a = -1)
    {
        if (a == -1)
        {
            a = alpha;
        }
        Color color = possibleColors[i];
        if (!fullOpacity)
            color.a = a;
        color.a = Mathf.Clamp01(color.a);
        return color;
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

    #region GENERAL PUBLIC

    public void SetActiveShadow(bool disable)
    {
        shadow.enabled = disable;

        doubleJump.enabled = disable;
        doubleJumpSpray.enabled = disable;
        bigDoubleJumpSpray.enabled = disable;

        inWater = !disable;
        splash.enabled = !disable;
        if (!disable)
        {
            PlayVFX("splash");
            landingBubbles.enabled = disable;
        }
        else
        {
            StartCoroutine(DisableBubbles());
        }
    }

    IEnumerator DisableBubbles()
    {
        freshOutOfWater = true;
        yield return new WaitForSeconds(1.4f);
        landingBubbles.enabled = true;
        freshOutOfWater = false;
    }
    public void PlayVFX(string effectName)
    {
        VisualEffect vfx;
        switch (effectName)
        {
            case "cloud":
                vfx = upgradeCloud;
                break;
            case "splash":
                float jumpIntensity = Mathf.Clamp(inAirTimer, minJumpTime, maxJumpTime);
                jumpIntensity = ExtensionMethods.Remap(jumpIntensity, minJumpTime, maxJumpTime, 0, 1);
                float curvepoint = 2.5f * decalScale;
                float curvepoint2 = 8 * decalScale;
                curvepoint = Mathf.Lerp(curvepoint, curvepoint2, jumpIntensity);
                splash.SetFloat("_Radius", curvepoint);
                vfx = splash;
                break;
            case "stepLeft":
                vfx = stepLeft;
                break;
            case "stepRight":
                vfx = stepRight;
                break;
            case "snap":
                vfx = snappingVFX;
                Vector3 snappingPoint = pSM.closestRail.pathCreator.path.GetClosestPointOnPath(transform.GetChild(0).position);
                float distance = pSM.closestRail.pathCreator.path.GetClosestDistanceAlongPath(snappingPoint);
                vfx.transform.position = snappingPoint;
                vfx.transform.LookAt(snappingPoint + pSM.closestRail.pathCreator.path.GetNormalAtDistance(distance));
                break;
            case "trail":
                vfx = slidingTrail;
                break;
            case "speedlines":
                vfx = speedLinesSliding;
                break;
            default:
                vfx = new VisualEffect();
                Debug.Log("This doesnt exist");
                break;
        }
        vfx.SendEvent("_Start");
    }
    public void StopVFX(string effectName)
    {
        VisualEffect vfx;
        switch (effectName)
        {
            case "trail":
                vfx = slidingTrail;
                break;
            case "speedlines":
                vfx = speedLinesSliding;
                break;
            default:
                vfx = new VisualEffect();
                Debug.Log("This doesnt exist");
                break;
        }
        vfx.SendEvent("_End");
    }
    #endregion

    #region CHALLENGES
    public void PlayCogwheel(Transform parentObj)
    {
        VisualEffect vE = parentObj.GetComponentInChildren<VisualEffect>();
        Vector3 dirVector = new Vector3(pSM.slidingInput, 0, 0);
        vE.SetVector3("_CurrentSpeed", dirVector);
        vE.SetFloat("_Magnitude", pSM.currentSlidingSpeed);
        vE.SendEvent("_Start");
        StartCoroutine(LightUpWheel());
    }
    public void PlaySwitch(Transform parentObj)
    {
        VisualEffect vE = parentObj.GetComponentInChildren<VisualEffect>();
        vE.SendEvent("_Start");
    }

    IEnumerator LightUpWheel()
    {
        float startIntensity = 1000;
        float endIntensity = wheelIntensity;
        float timer = 0;
        wheelMat.SetVector("_Position", transform.GetChild(0).position);
        while (timer < 0.5f)
        {
            float t = timer / 0.5f;
            timer += Time.deltaTime;
            wheelMat.SetFloat("_Emission", Mathf.Lerp(startIntensity, endIntensity, t));
            yield return new WaitForEndOfFrame();
        }
        while (timer < 1.5f)
        {
            float t = timer / 1.5f;
            timer += Time.deltaTime;
            wheelMat.SetFloat("_Emission", Mathf.Lerp(endIntensity, startIntensity, t));
            yield return new WaitForEndOfFrame();
        }
        wheelMat.SetFloat("_Emission", startIntensity);
    }
    #endregion

    #region SHADOW
    IEnumerator OnImpact(float inAirTime)
    {
        int i = 0;
        float jumpIntensity = Mathf.Clamp(inAirTime, minJumpTime, maxJumpTime);
        jumpIntensity = ExtensionMethods.Remap(jumpIntensity, minJumpTime, maxJumpTime, 0, 1);
        if (!useFadeOut)
            shadow.material.SetColor("_BaseColor", GetColor(randomColor));
        else
            i = randomColor;
        float timer = 0;
        float time = impactCurve.keys[impactCurve.length - 1].time;
        bool castEffect = false;
        while (timer < time)
        {
            float t = timer / time;

            if (useFadeOut)
                shadow.material.SetColor("_BaseColor", GetColor(i, false, Mathf.Lerp(alpha, 0, t)));

            float curvepoint = impactCurve.Evaluate(t) * decalScale;
            float curvepoint2 = hardImpactCurve.Evaluate(t) * decalScale;
            curvepoint = Mathf.Lerp(curvepoint, curvepoint2, jumpIntensity);

            shadow.size = new Vector3(curvepoint, curvepoint, shadowRemapMax);

            if (t >= 0.2f && !castEffect)
            {
                if (!inWater && !freshOutOfWater)
                {
                    landingBubbles.SetFloat("_Radius", curvepoint);
                    landingBubbles.SetVector4("_Color", GetColor(randomColor));
                    landingBubbles.SendEvent("_Start");
                }
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

        int j = 0;
        float timer = 0;
        float time = impactCurve.keys[impactCurve.length - 1].time;
        bool castEffect = false;

        if (!useFadeOut)
            doubleJump.material.SetColor("_BaseColor", GetColor(randomColor));
        else
            j = randomColor;

        while (timer < time)
        {
            doubleJump.transform.LookAt(doubleJump.transform.position - planeNormal);
            float t = timer / time;

            if (useFadeOut)
                doubleJump.material.SetColor("_BaseColor", GetColor(j, false, Mathf.Lerp(alpha, 0, t)));

            float curvepoint = impactCurve.Evaluate(t) * decalScale;
            float curvepoint2 = hardImpactCurve.Evaluate(t) * decalScale;
            curvepoint = Mathf.Lerp(curvepoint, curvepoint2, jumpIntensity) * 1.7f;

            doubleJump.size = new Vector3(curvepoint, curvepoint, shadowRemapMax);
            bigDoubleJumpSpray.transform.position = new Vector3(doubleJumpSpray.transform.position.x, sprayY, doubleJumpSpray.transform.position.z);
            if (t >= 0.2f && !castEffect)
            {
                int i = randomColor;
                doubleJumpSpray.SetFloat("_Radius", curvepoint);
                doubleJumpSpray.SetVector4("_Color", GetColor(i));
                doubleJumpSpray.SetVector3("_Normal", planeNormal);
                doubleJumpSpray.SetVector3("_Up", planeUp);
                doubleJumpSpray.SendEvent("_Start");

                bigDoubleJumpSpray.SetFloat("_Radius", curvepoint);
                bigDoubleJumpSpray.SetVector4("_Color", GetColor(i));
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
        int i = 0;
        if (!useFadeOut)
            wallProjector.material.SetColor("_BaseColor", GetColor(randomColor));
        else
            i = randomColor;

        if (!useNewWallProjector)
        {
            wallProjector.material.SetColor("_Color", GetColor(randomColor, true));
        }

        wallProjector.transform.position = pSM.ladder.transform.position + Vector3.up * wallOffsetUp + ladder.transform.forward * wallOffsetBack;
        lastPositionWall = pSM.ladder.transform.position + Vector3.up * wallOffsetUp + ladder.transform.forward * wallOffsetBack;
        wallProjector.transform.rotation = Quaternion.Euler(wallProjector.transform.eulerAngles.x, ladder.transform.eulerAngles.y, wallProjector.transform.eulerAngles.z);
        float timer = 0;
        bool once = false;
        while (timer < time)
        {
            float t = timer / time;
            if (pSM.playerState == PlayerMovementStateMachine.PlayerState.swinging)
                lastPositionWall = pSM.ladder.transform.position + Vector3.up * wallOffsetUp + ladder.transform.forward * wallOffsetBack;
            if (!useNewWallProjector)
            {
                float currentTime = Mathf.Lerp(0.2f, 1.22f, t);
                wallProjector.material.SetFloat("_WallTime", currentTime);
            }
            else
            {
                float curvepoint = impactCurve.Evaluate(t) * decalScale * 1.75f;
                wallProjector.size = new Vector3(curvepoint, curvepoint, shadowRemapMax);
                if (useFadeOut)
                    wallProjector.material.SetColor("_BaseColor", GetColor(i, false, Mathf.Lerp(2, 0, t)));
                if (t >= 0.2f && !once)
                {
                    wallBubbles.SetFloat("_Radius", curvepoint);
                    wallBubbles.SetVector3("_Normal", wallProjector.transform.forward);
                    wallBubbles.SetVector3("_Up", wallProjector.transform.up);
                    wallBubbles.SetVector4("_Color", GetColor(randomColor));
                    wallBubbles.SendEvent("_Start");
                    once = true;
                }
            }
            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        if (!useNewWallProjector)
        {
            wallProjector.material.SetFloat("_WallTime", 0);
        }
        else
        {
            wallProjector.size = new Vector3(0, 0, shadowRemapMax);
        }
        wallProjecting = false;
    }
    #endregion

    #region WATER STEPS
    public void TriggerLeftFoot()
    {
        // if (inWater)
        //     StartCoroutine(ExtendWater("left"));
        // else
        //     PlayVFX("stepLeft");
    }
    public void TriggerRightFoot()
    {
        // if (inWater)
        //     StartCoroutine(ExtendWater("right"));
        // else
        //     PlayVFX("stepRight");
    }
    IEnumerator ExtendWater(string side)
    {
        float timer = 0;
        while (timer < waterSpeed)
        {
            float t = timer / waterSpeed;
            float currentSize = Mathf.Lerp(0, 0.6f, t);
            if (side == "left")
            {
                waterStepsLeft.size = new Vector3(currentSize, currentSize, 1);
                waterStepsLeft.material.SetFloat("_Alpha", Mathf.Lerp(1, 0, t));
            }
            else if (side == "right")
            {
                waterStepsRight.size = new Vector3(currentSize, currentSize, 1);
                waterStepsRight.material.SetFloat("_Alpha", Mathf.Lerp(1, 0, t));
            }

            timer += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        if (side == "left")
        {
            waterStepsLeft.size = new Vector3(0, 0, 1);
            waterStepsLeft.material.SetFloat("_Alpha", 1);
        }
        else if (side == "right")
        {
            waterStepsRight.size = new Vector3(0, 0, 1);
            waterStepsRight.material.SetFloat("_Alpha", 1);
        }

    }
    #endregion

    #region ANSNAPPEN
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

    #region ENVIRONMENT
    public IEnumerator TriggerStartWaterfall(float waterfallHeight, GameObject waterfall, GameObject waterfallFoam)
    {
        Material water = new Material(waterfall.GetComponent<MeshRenderer>().material);
        water.SetFloat("_Height", waterfallHeight);
        waterfall.GetComponent<MeshRenderer>().material = water;
        float timer = 0;
        float t = 0;
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        while (timer < waterfallTime) // fills up the waterfall
        {
            t = timer / waterfallTime;
            water.SetFloat("_Time", t);
            timer += Time.deltaTime;
            yield return delay;
        }
        waterfallFoam.SetActive(true);
    }
    public IEnumerator MoveWindIn(GameObject construct)
    {
        float generalTime = 1;
        wind.SendEvent("_Start");
        Vector3 startPosition = player.transform.position;
        Vector3 endPosition = construct.transform.position;
        Vector3 startPosition2 = wind.transform.localPosition;
        Vector3 endPosition2 = new Vector3(2, 0, 0);
        WaitForEndOfFrame delay = new WaitForEndOfFrame();
        float timer = 0;
        float t = 0;
        windParent.transform.position = startPosition;

        // move the wind to the construct
        float movetime = 1 * generalTime;
        while (timer < movetime)
        {
            t = timer / movetime;
            windParent.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            wind.transform.localPosition = Vector3.Lerp(startPosition2, endPosition2, t);
            timer += Time.deltaTime;
            yield return delay;
        }

        timer = 0;
        float rotateTime = 2 * generalTime;
        Vector3 startEuler = wind.transform.parent.eulerAngles;
        Vector3 endEuler = new Vector3(0, 1080, 0);
        startPosition = construct.transform.position;
        endPosition = construct.transform.position + Vector3.up * 2;
        startPosition2 = wind.transform.localPosition;
        endPosition2 = new Vector3(0, 0, 0);

        // move the wind around the construct
        // move the wind back to the parent
        while (timer < rotateTime)
        {
            t = timer / rotateTime;
            wind.transform.parent.eulerAngles = Vector3.Lerp(startEuler, endEuler, t);
            windParent.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            wind.transform.localPosition = Vector3.Lerp(startPosition2, endPosition2, t);
            timer += Time.deltaTime;
            yield return delay;
        }

        timer = 0;
        float landmarkTime = 1.2f * generalTime;
        startPosition = windParent.transform.position;
        endPosition = (nextLandmark.transform.position - windParent.transform.position).normalized * 10 + windParent.transform.position;

        // move the wind towards the next landmark
        while (timer < landmarkTime)
        {
            t = timer / landmarkTime;
            windParent.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            timer += Time.deltaTime;
            yield return delay;
        }
        wind.SendEvent("_End");
    }
    #endregion
}
