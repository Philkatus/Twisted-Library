using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using PathCreation;

public class PlayerMovementStateMachine : StateMachine
{
    #region public
    [Header("Changeable")]
    [Tooltip("Change to use different variable value sets. Found in Assets-> Scripts-> Cheat Sheets.")]
    public ValuesScriptableObject stats;
    public InputActionAsset actionAsset;
    public bool useRelativeBobPosition = true;

    [Space]
    [Header("For reference")]
    public PlayerState playerState;
    public LadderState ladderState;
    [Space]

    public float HeightOnLadder = -1;
    public float currentDistance;
    public float sideWaysInput;
    public float forwardInput;
    public float slideLeftInput;
    public float slideRightInput;
    public float slidingInput;
    public float startingSlidingInput;
    public float currentSlidingSpeed;

    public bool dismounting;
    public bool didLadderPush;
    public bool isWallJumping;
    public bool animationControllerisFoldingJumped;
    public bool expandAfterSnap;

    public bool isOnWater;
    public bool isOnGras;
    public bool controlsDisabled;

    public Vector3 baseVelocity;
    public Vector3 bonusVelocity;
    public Vector3 playerVelocity
    {
        get
        {
            return baseVelocity + bonusVelocity;
        }
        set
        {
            baseVelocity = value;
        }

    }
    public Vector3 railCheckLadderPosition;

    public Rail closestRail;
    public Transform ladder;
    public LadderSizeStateMachine ladderSizeStateMachine;
    public CharacterController controller;
    public AnimationStateController animController;
    public GameObject bob;
    public Transform Bob_Pivot;
    public VFX_Manager effects;
    [HideInInspector] public InputAction slideLeftAction;
    [HideInInspector] public InputAction slideRightAction;
    [HideInInspector] public InputAction swingAction;
    [HideInInspector] public InputAction snapAction;
    [HideInInspector] public InputAction fallFromLadder;
    [HideInInspector] public Quaternion ladderWalkingRotation;
    [HideInInspector] public Vector3 ladderWalkingPosition;
    [HideInInspector] public Vector3 ladderJumpTarget;
    [HideInInspector]
    public Vector3 ladderDirection
    {
        get
        {
            return ladderSizeStateMachine.ladderParent.right;
        }
    }
    [HideInInspector] public int snapdirection = 1;

    #region inputBools
    bool[] inputBools = new bool[4];
    public bool jumpInputBool
    {
        get
        {
            return inputBools[0];
        }
        set
        {
            inputBools[0] = value;
        }

    }
    public bool snapInputBool
    {
        get
        {
            return inputBools[1];
        }
        set
        {
            inputBools[1] = value;
        }

    }
    public bool foldInputBool
    {
        get
        {
            return inputBools[2];
        }
        set
        {
            inputBools[2] = value;
        }

    }
    public bool swingInputBool
    {
        get
        {
            return inputBools[3];
        }
        set
        {
            inputBools[3] = value;
        }

    }
    #endregion

    public float coyoteTimer = 0;

    [HideInInspector] public Transform myParent;
    #endregion
    #region Private
    float railCheckTimer;
    RailSearchManager railAllocator;
    InputActionMap playerControlsMap;
    InputAction jumpAction;
    InputAction moveAction;
    InputAction foldAction;
    public Rail lastRail;

    Coroutine[] inputTimer = new Coroutine[4];
    #endregion

    private void Awake()
    {
        ObjectManager.instance.pSM = this;
    }
    private void Start()
    {
        InitializeVariables();
        SetState(new PlayerWalking(this));
        GetControls();
    }

    private void Update()
    {
        coyoteTimer += Time.deltaTime;
        if (playerState != PlayerState.swinging)
            UpdateRailTimer();
        if (!controlsDisabled)
        {
            CheckForInputBools();
        }
        if (playerState == PlayerState.swinging && currentSlidingSpeed >= stats.maxSlidingSpeed * .8f)
        {
            if (VoiceManager.Instance != null)
            {
                VoiceManager.Instance.TryToHighSpeedSound();
            }
        }
    }

    private void FixedUpdate()
    {
        if (!controlsDisabled)
        {
            GetInput();
            //LooseBonusVelocity(stats.bonusVelocityDrag,Vector3.up);
            State.Movement();
        }
    }

    private void InitializeVariables()
    {
        myParent = transform.parent;
        railAllocator = RailSearchManager.instance;
        ladderWalkingPosition = ladder.localPosition;
        ladderWalkingRotation = ladder.localRotation;
        coyoteTimer = stats.slidingCoyoteTime;
    }

    private void UpdateRailTimer()
    {
        railCheckTimer += Time.deltaTime;
        if (railCheckTimer >= 0.07f)
        {
            CheckForRail();
            railCheckTimer = 0;
        }
    }

    public void TryToSnapToShelf()
    {
        if (playerState != PlayerState.swinging && CheckForRail())
        {
            State.Snap();
        }
    }

    #region Input/Controls
    public void GetInput()
    {
        forwardInput = moveAction.ReadValue<Vector2>().y;
        sideWaysInput = moveAction.ReadValue<Vector2>().x;
        if (!stats.useInvertedSliding)
        {
            slideLeftInput = slideLeftAction.ReadValue<float>();
            slideRightInput = slideRightAction.ReadValue<float>();
        }
        else
        {
            slideLeftInput = slideRightAction.ReadValue<float>();
            slideRightInput = slideLeftAction.ReadValue<float>();
        }
    }

    public void SaveInput(int index, float duration)
    {
        if (inputTimer[index] != null)
        {
            StopCoroutine(inputTimer[index]);
        }
        inputTimer[index] = StartCoroutine(InputTimer(index, duration));
    }
    public void SaveInput(int index, float duration, Rail rail)
    {
        if (inputTimer[index] != null)
        {
            StopCoroutine(inputTimer[index]);
        }
        inputTimer[index] = StartCoroutine(InputTimer(index, duration, rail));
    }

    private void CheckForInputBools()
    {
        if (jumpInputBool)
        {
            State.Jump();
            if (stats.useJumpForLadderPush && jumpInputBool)
            {
                State.LadderPush();
            }

        }
        if (snapInputBool && playerState != PlayerState.swinging)
        {
            TryToSnapToShelf();
        }
        if (foldInputBool && stats.canLadderFold)
        {
            if (!stats.useJumpForLadderPush)
            {
                State.LadderPush();
            }
            ladderSizeStateMachine.OnFold();
        }
    }

    IEnumerator InputTimer(int index, float duration)
    {
        inputBools[index] = true;
        yield return new WaitForSeconds(duration);
        inputBools[index] = false;
    }

    IEnumerator InputTimer(int index, float duration, Rail lastRail)
    {
        this.lastRail = lastRail;
        yield return new WaitForSeconds(.34f);
        if (slideRightInput != 0 || slideLeftInput != 0)
        {
            inputBools[index] = true;
        }
        yield return new WaitForSeconds(duration);
        inputBools[index] = false;
        this.lastRail = null;
    }
    private void GetControls()
    {
        playerControlsMap = actionAsset.FindActionMap("PlayerControls");
        slideLeftAction = playerControlsMap.FindAction("SlideLeft");
        slideRightAction = playerControlsMap.FindAction("SlideRight");
        slideLeftAction.started += context => { if (playerState != PlayerState.swinging) { startingSlidingInput = -1; } };
        slideRightAction.started += context => { if (playerState != PlayerState.swinging) { startingSlidingInput = +1; } };
        slideRightAction.canceled += context => { if (playerState != PlayerState.swinging) { startingSlidingInput = 0; } };
        slideLeftAction.canceled += context => { if (playerState != PlayerState.swinging) { startingSlidingInput = 0; } };
        if (stats.useTriggerToSlideWithMomentum)
        {
            slideLeftAction.started += context => SaveInput(1, stats.snapInputTimer);
            slideRightAction.started += context => SaveInput(1, stats.snapInputTimer);
        }
        startingSlidingInput = 0;

        if (GameObject.FindGameObjectWithTag("Canvas"))
        {
            playerControlsMap.Disable();
        }
        else
        {
            playerControlsMap.Enable();
        }
        jumpAction = playerControlsMap.FindAction("Jump");
        moveAction = playerControlsMap.FindAction("Movement");
        snapAction = playerControlsMap.FindAction("Snap");
        swingAction = playerControlsMap.FindAction("Swing");
        foldAction = playerControlsMap.FindAction("Fold");
        fallFromLadder = playerControlsMap.FindAction("FallFromLadder");
        fallFromLadder.performed += context => State.FallFromLadder();
        jumpAction.performed += context => SaveInput(0, stats.jumpInputTimer);
        snapAction.performed += context => SaveInput(1, stats.snapInputTimer);
        foldAction.performed += context => SaveInput(2, stats.foldInputTimer);
        swingAction.performed += context => SaveInput(3, stats.swingInputTimer);
    }
    #endregion
    #region utility
    public void LooseBonusVelocity(float dragAmount)
    {
        bonusVelocity -= bonusVelocity.normalized * dragAmount * Time.fixedDeltaTime;
        if (bonusVelocity.magnitude <= dragAmount * Time.fixedDeltaTime)
        {
            bonusVelocity = Vector3.zero;
        }
    }
    public void LooseBonusVelocity(float dragAmount, Vector3 axis)
    {
        bonusVelocity -= axis.normalized * dragAmount * Time.fixedDeltaTime;
        if (bonusVelocity.magnitude <= dragAmount * Time.fixedDeltaTime)
        {
            bonusVelocity = Vector3.zero;
        }
    }
    public void LoseBonusVelocityPercentage(float dragAmount)
    {
        dragAmount = (100 - dragAmount) / 100;
        bonusVelocity *= dragAmount * Time.fixedDeltaTime;
        if (bonusVelocity.magnitude <= Time.fixedDeltaTime)
        {
            bonusVelocity = Vector3.zero;
        }
    }

    public void LoseBonusVelocityPercentage(float dragAmount, Vector3 axis)
    {
        dragAmount = (100 - dragAmount) / 100;
        Vector3 resultingVelocity = ExtensionMethods.resultingVelocity(bonusVelocity, axis);
        bonusVelocity -= resultingVelocity;
        resultingVelocity *= dragAmount * Time.fixedDeltaTime;
        if (resultingVelocity.magnitude <= Time.fixedDeltaTime)
        {
            resultingVelocity = Vector3.zero;
        }
        bonusVelocity += resultingVelocity;
    }
    ///<summary>
    /// A function to determine the closest rail to the player. Returns false if none are in range.
    ///</summary>
    public bool CheckForRail()
    {
        if (playerState == PlayerState.walking)
        {
            railCheckLadderPosition = controller.transform.position;
        }
        else if (playerState == PlayerState.inTheAir)
        {
            railCheckLadderPosition = controller.transform.position;
        }

        railAllocator.CheckForRailsInRange(controller.transform);
        var possibleRails = railAllocator.railsInRange;
        List<Rail> lessPossibleRails = new List<Rail>();

        if (possibleRails.Count == 0)
        {
            return false;
        }
        else
        {
            float closestDistance = stats.snappingDistance;
            for (int i = 0; i < possibleRails.Count; i++)
            {
                Vector3 snappingPoint = possibleRails[i].pathCreator.path.GetClosestNotConcealedPointOnPathData(railCheckLadderPosition);
                float distance = Vector3.Distance(snappingPoint, railCheckLadderPosition);
                if (distance >= closestDistance || possibleRails[i] == lastRail)
                {
                    possibleRails.Remove(possibleRails[i]);
                    i--;
                }
            }
            for (int i = 0; i < possibleRails.Count; i++)
            {
                Vector3 snappingPoint = possibleRails[i].pathCreator.path.GetClosestNotConcealedPointOnPathData(railCheckLadderPosition);
                LayerMask mask = LayerMask.GetMask("Environment");
                RaycastHit hit;
                if (Physics.Linecast(Camera.main.transform.position, snappingPoint, out hit, mask, QueryTriggerInteraction.Ignore))
                {
                    lessPossibleRails.Add(possibleRails[i]);
                    possibleRails.Remove(possibleRails[i]);
                    i--;
                }
            }
            bool onlyObscuredRails = false;
            if (possibleRails.Count == 0)
            {
                onlyObscuredRails = true;
                possibleRails.AddRange(lessPossibleRails);
            }

            for (int i = 0; i < possibleRails.Count; i++)
            {
                Vector3 snappingPoint;
                if (onlyObscuredRails)
                {
                    snappingPoint = possibleRails[i].pathCreator.path.GetClosestPointOnPath(railCheckLadderPosition);
                }
                else
                {
                    snappingPoint = possibleRails[i].pathCreator.path.GetClosestNotConcealedPointOnPathData(railCheckLadderPosition);
                }
                float distance = Vector3.Distance(snappingPoint, railCheckLadderPosition);
                LayerMask mask = LayerMask.GetMask("Environment");

                if (distance < closestDistance)
                {
                    RaycastHit hit;
                    if (!Physics.Linecast(railCheckLadderPosition, snappingPoint, out hit, mask, QueryTriggerInteraction.Ignore))
                    {

                        closestRail = possibleRails[i];
                        closestDistance = distance;
                    }
                    if (hit.point != new RaycastHit().point && Vector3.Distance(snappingPoint, hit.point) <= .1f)
                    {

                        closestRail = possibleRails[i];
                        closestDistance = distance;
                    }
                }
            }
            if (closestDistance >= stats.snappingDistance)
            {
                closestRail = null;
            }

            //VFX-Snapping
            effects.currentRail = closestRail;

            if (closestRail != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    ///<summary>
    /// A function to determine the closest rail to the player that ignores the current rail. Return false if none are in range.
    ///</summary>
    public bool CheckForNextClosestRail(Rail currentRail)
    {
        railCheckLadderPosition = ladder.transform.position;
        railAllocator.CheckForRailsInRange(ladder.transform);
        var possibleRails = railAllocator.railsInRange;

        if (possibleRails.Count == 1)
        {

            return false;
        }
        else
        {

            //finding of the direction  of the current rail
            VertexPath currentPath = currentRail.pathCreator.path;
            Vector3 currentDirection = currentPath.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);

            float closestDistance = stats.resnappingDistance;
            Rail nextClosestRail = null;
            for (int i = 0; i < possibleRails.Count; i++)
            {
                float distance = Vector3.Distance(possibleRails[i].pathCreator.path.GetClosestPointOnPath(currentPath.GetPointAtDistance(currentDistance, EndOfPathInstruction.Stop)), currentPath.GetPointAtDistance(currentDistance, EndOfPathInstruction.Stop));
                VertexPath possiblePath = possibleRails[i].pathCreator.path;
                Vector3 possiblePathDirection = possiblePath.GetDirectionAtDistance(
                possiblePath.GetClosestDistanceAlongPath(currentPath.GetPointAtDistance(currentDistance, EndOfPathInstruction.Stop)), EndOfPathInstruction.Stop);

                if (distance < closestDistance
                    && possibleRails[i] != currentRail)
                {

                    if (Mathf.Abs(Vector3.Dot(currentDirection.normalized, possiblePathDirection.normalized)) > stats.resnappingDotProduct) // hab das >= zu einem > 0 gemacht erstmal, falls sich das gerade jmd ansieht. jetzt geht es einigerma�en
                    {
                        closestDistance = distance;
                        nextClosestRail = possibleRails[i];
                    }

                }

            }

            if (nextClosestRail != null)
            {
                float pathlength = closestRail.pathCreator.path.cumulativeLengthAtEachVertex[closestRail.pathCreator.path.cumulativeLengthAtEachVertex.Length - 1];
                if (currentDistance >= pathlength)
                    currentDistance -= pathlength;
                else
                    currentDistance = pathlength + currentDistance;
                closestRail = nextClosestRail;

                //VFX-Snapping
                effects.currentRail = closestRail;

                return true;
            }
            else
            {

                return false;
            }
        }
    }
    #endregion

    #region functions to change states
    ///<summary>
    /// Gets called when the player lands on the floor.
    ///</summary>
    public void OnLand()
    {
        SetState(new PlayerWalking(this));
        effects.OnStateChangedWalking(true);
        playerState = PlayerState.walking;
        HeightOnLadder = -1;
    }

    ///<summary>
    /// Gets called when the player leaves the ladder on the top.
    ///</summary>
    public void OnLadderTop()
    {
        SetState(new PlayerWalking(this));
        effects.OnStateChangedWalking(false);
        playerState = PlayerState.walking;
        ladderSizeStateMachine.OnShrink();
    }

    ///<summary>
    /// Gets called when the player leaves the ladder on the bottom.
    ///</summary>
    public void OnLadderBottom()
    {
        SetState(new PlayerInTheAir(this));
        effects.OnStateChangedInAir();
        playerState = PlayerState.inTheAir;
        ladderSizeStateMachine.OnShrink();
    }

    ///<summary>
    /// Gets called when the player snaps his ladder to a rail.
    ///</summary>
    public void OnSnap()
    {

        snapInputBool = false;
        effects.OnStateChangedSwinging();
        playerState = PlayerState.swinging;
        SetState(new PlayerSwinging(this));

    }

    ///<summary>
    /// Gets called when the player snaps to the next path.
    ///</summary>
    public void OnResnap()
    {
        SetState(this.State);
        effects.OnResnap();
        playerState = PlayerState.swinging;
    }

    ///<summary>
    /// Gets called when the player changes to in the air.
    ///</summary>
    public void OnFall()
    {
        SetState(new PlayerInTheAir(this));
        effects.OnStateChangedInAir();
        playerState = PlayerState.inTheAir;
        ladderSizeStateMachine.OnShrink();
        HeightOnLadder = -1;
    }
    //Gets called, when the Player is being catapulted out of Landmark 1
    //if this is at the wrong place just move it
    public void Jump()
    {
        State.Jump();
    }
    #endregion

    public enum PlayerState
    {
        walking,
        inTheAir,
        swinging
    };

    public enum LadderState
    {
        LadderBig,
        LadderSmall,
        LadderFold,
        LadderUnfold,
        LadderPush,
        LadderSnap
    };
}
