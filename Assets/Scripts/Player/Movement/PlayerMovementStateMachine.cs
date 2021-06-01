using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using PathCreation;

public class PlayerMovementStateMachine : StateMachine
{
    #region public
    [Header("Changeable")]
    [Tooltip("Change to use different variable value sets. Found in Assets-> Scripts-> Cheat Sheets.")]
    public ValuesScriptableObject stats;
    public InputActionAsset actionAsset;

    [Space]
    [Header("For reference")]
    public PlayerState playerState;
    public LadderState ladderState;
    [Space]

    public float HeightOnLadder = -1;
    public float currentDistance;
    public float sideWaysInput;
    public float forwardInput;
    public float swingingInput;
    public float slidingInput;
    public float startingSlidingInput;
    public int adjustedSlideDirection;
    public bool dismounting;
    public bool didLadderPush;

    public bool isWallJumping;
    public bool animationControllerisFoldingJumped;

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
    public VFX_Manager effects;
    [HideInInspector] public InputAction slideAction;
    [HideInInspector] public InputAction slideLeftAction;
    [HideInInspector] public InputAction slideRightAction;
    [HideInInspector] public InputAction swingAction;
    [HideInInspector] public InputAction snapAction;
    [HideInInspector] public InputAction stopSlidingAction;
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
    Vector3 lastVisualizationPoint;

    GameObject snapVisualisation;
    RailSearchManager railAllocator;
    InputActionMap playerControlsMap;
    InputAction jumpAction;
    InputAction moveAction;
    InputAction foldAction;

    Coroutine[] inputTimer = new Coroutine[4];
    #endregion

    private void Start()
    {
        InitializeReferences();
        SetState(new PlayerWalking(this));
        GetControlls();
    }



    private void Update()
    {
        coyoteTimer += Time.deltaTime;
        UpdateRailTimer();
        CheckForInputBools();

    }



    private void FixedUpdate()
    {
        GetInput();
        looseBonusVelocity(stats.bonusVelocityDrag);
        State.Movement();
        Debug.DrawRay(transform.position, playerVelocity, Color.magenta);
        Debug.DrawRay(transform.position, bonusVelocity, Color.blue);
    }

    public void TryToSnapToShelf()
    {
        if (CheckForRail())
        {
            State.Snap();
        }
    }

    private void InitializeReferences()
    {
        myParent = transform.parent;
        railAllocator = RailSearchManager.instance;
        ladderWalkingPosition = ladder.localPosition;
        ladderWalkingRotation = ladder.localRotation;
        snapVisualisation = myParent.transform.GetChild(3).GetChild(1).gameObject;
        coyoteTimer = stats.slidingCoyoteTime;
    }
    private void UpdateRailTimer()
    {
        railCheckTimer += Time.deltaTime;
        if (railCheckTimer >= 0.1f)
        {
            CheckForRail();
            if (playerState != PlayerState.swinging)
            {
                StartCoroutine(ChangeSnapVisualisationPoint());
            }
            else
            {
                snapVisualisation.SetActive(false);
            }
            railCheckTimer = 0;
        }
    }

    #region utility
    #region Input/Controlls
    public void GetInput()
    {
        forwardInput = moveAction.ReadValue<Vector2>().y;
        sideWaysInput = moveAction.ReadValue<Vector2>().x;
        swingingInput = swingAction.ReadValue<float>();
    }

    public void SaveInput(int index, float duration)
    {
        // Debug.Log("saveIput"+index);
        if (inputTimer[index] != null)
        {
            StopCoroutine(inputTimer[index]);
        }
        inputTimer[index] = StartCoroutine(InputTimer(index, duration));
    }

    private void CheckForInputBools()
    {
        if (jumpInputBool)
        {
            if (stats.useJumpForLadderShoot)
            {
                State.LadderPush();
            }
            State.Jump();
        }
        if (snapInputBool)
        {
            TryToSnapToShelf();
        }
        if (foldInputBool)
        {
            if (!stats.useJumpForLadderShoot)
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

    private void GetControlls()
    {
            playerControlsMap = actionAsset.FindActionMap("PlayerControlsNewSliding");
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

        jumpAction.performed += context => SaveInput(0, stats.jumpInputTimer);// State.Jump();
        snapAction.performed += context => SaveInput(1, stats.snapInputTimer);   //TryToSnapToShelf();
        foldAction.performed += context => SaveInput(2, stats.foldInputTimer); //ladderSizeStateMachine.OnFold();
        swingAction.performed += context => SaveInput(3, stats.swingInputTimer);
        //foldAction.performed += context => State.RocketJump();
    }
    #endregion
    public void looseBonusVelocity(float dragAmount)
    {
        bonusVelocity -= bonusVelocity.normalized * dragAmount * Time.fixedDeltaTime;
        if (bonusVelocity.magnitude <= dragAmount * Time.fixedDeltaTime)
        {
            bonusVelocity = Vector3.zero;
        }
    }
    public void loseBonusVelocityPercentage(float dragAmount)
    {
        dragAmount = (100 - dragAmount) / 100;
        bonusVelocity *= dragAmount * Time.fixedDeltaTime;
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

        if (possibleRails.Count == 0)
        {
            return false;
        }
        else
        {
            float closestDistance = stats.snappingDistance;
            for (int i = 0; i < possibleRails.Count; i++)
            {
                Vector3 snappingPoint = possibleRails[i].pathCreator.path.GetClosestPointOnPath(railCheckLadderPosition);
                float distance = Vector3.Distance(snappingPoint, railCheckLadderPosition);
                LayerMask mask = LayerMask.GetMask("Environment");

                if (distance < closestDistance)
                {
                    Debug.DrawLine(railCheckLadderPosition, snappingPoint, Color.blue);
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
    public bool CheckForNextClosestRail(Rail currentClosestRail)
    {
        railCheckLadderPosition = ladder.transform.position;
        railAllocator.CheckForRailsInRange(controller.transform);
        var possibleRails = railAllocator.railsInRange;

        if (possibleRails.Count == 1)
        {
            return false;
        }
        else
        {
            //finding of the direction  of the current rail
            VertexPath currentClosestPath = currentClosestRail.pathCreator.path;
            Vector3 currentDirection = currentClosestPath.GetDirectionAtDistance(currentDistance, EndOfPathInstruction.Stop);

            float closestDistance = stats.resnappingDistance;
            Rail nextClosestRail = null;

            for (int i = 0; i < possibleRails.Count; i++)
            {
                float distance = Vector3.Distance(possibleRails[i].pathCreator.path.GetClosestPointOnPath(railCheckLadderPosition), railCheckLadderPosition);
                VertexPath possiblePath = possibleRails[i].pathCreator.path;
                Vector3 possiblePathDirection = possiblePath.GetDirectionAtDistance(
                possiblePath.GetClosestDistanceAlongPath(currentClosestPath.GetPointAtDistance(currentDistance, EndOfPathInstruction.Stop)), EndOfPathInstruction.Stop);

                if (distance < closestDistance
                    && possibleRails[i] != currentClosestRail)
                //&& possibleRails[i].transform.position.y == currentClosestRail.transform.position.y)
                {
                    if (Mathf.Abs(Vector3.Dot(currentDirection, possiblePathDirection)) >= .99f)
                    {
                        closestDistance = distance;
                        nextClosestRail = possibleRails[i];
                    }
                }
            }

            if (nextClosestRail != null)
            {
                closestRail = nextClosestRail;
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
        effects.OnStateChangedWalking();
        playerState = PlayerState.walking;
        HeightOnLadder = -1;
    }

    ///<summary>
    /// Gets called when the player leaves the ladder on the top.
    ///</summary>
    public void OnLadderTop()
    {
        SetState(new PlayerWalking(this));
        playerState = PlayerState.walking;
        ladderSizeStateMachine.OnShrink();
    }

    ///<summary>
    /// Gets called when the player leaves the ladder on the bottom.
    ///</summary>
    public void OnLadderBottom()
    {
        SetState(new PlayerInTheAir(this));
        playerState = PlayerState.inTheAir;
        ladderSizeStateMachine.OnShrink();
    }

    ///<summary>
    /// Gets called when the player snaps his ladder to a rail.
    ///</summary>
    public void OnSnap()
    {
        ladderSizeStateMachine.OnGrow();
        snapInputBool = false;

        SetState(new PlayerSwinging(this));
        playerState = PlayerState.swinging;
    }

    ///<summary>
    /// Gets called when the player snaps to the next path.
    ///</summary>
    public void OnResnap()
    {

        SetState(this.State);
        playerState = PlayerState.swinging;
    }

    ///<summary>
    /// Gets called when the player changes to in the air.
    ///</summary>
    public void OnFall()
    {
        SetState(new PlayerInTheAir(this));
        playerState = PlayerState.inTheAir;
        ladderSizeStateMachine.OnShrink();
        HeightOnLadder = -1;
    }
    #endregion

    #region VFX
    IEnumerator ChangeSnapVisualisationPoint()
    {
        float timer = 0;
        float t = 0;
        Vector3 nextPosition;

        if (closestRail != null && playerState != PlayerState.swinging)
        {
            nextPosition = closestRail.pathCreator.path.GetClosestPointOnPath(transform.position);
            snapVisualisation.SetActive(true);
        }
        else
        {
            nextPosition = Vector3.zero;
            snapVisualisation.SetActive(false);
        }

        while (timer < 0.1f)
        {
            timer += Time.deltaTime;
            t = timer / 0.1f;
            t = Mathf.Clamp(t, 0, 1);
            if (closestRail != null)
            {
                snapVisualisation.SetActive(true);
                snapVisualisation.transform.position = Vector3.Lerp(lastVisualizationPoint, nextPosition, t);
            }

            yield return new WaitForEndOfFrame();
        }

        lastVisualizationPoint = nextPosition;
        yield return true;
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
        LadderRocketJump
    };
}
