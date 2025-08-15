using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[System.Flags]
public enum CollisionFlags2D
{
    Right, // 1 = 1 << 0
    Above, // 2 = 1 << 1
    Left, // 4 = 1 << 2 
    Below // 8 = 1 << 3
}

public class CharacterController2D : MonoBehaviour
{
    [Header("Component References")]
    public CharacterProfile characterProfile;
    public Transform self;
    public CharacterRaycaster2D raycaster;
    public Animator animator;
    public Transform graphicTransform;

    public float graphicMargin;

    [Header("Events")]
    public UnityEvent onFell;
    public UnityEvent onGrounded, onHurt;
    public UnityEvent<int> onJumped;
    public UnityEvent<CollisionFlags2D> onCollisionStay;


    [System.NonSerialized] public bool isGrounded;
    [System.NonSerialized] public bool isJumping;
    [System.NonSerialized] public int wallJumpDirection = 0;
    [System.NonSerialized] public bool isWallSliding;
    [System.NonSerialized] public bool isUnderCoyoteTime;
    [System.NonSerialized] public float jumpTimestamp;
    [System.NonSerialized] public float coyoteTimestamp;
    [System.NonSerialized] public int remainingJumps;
    [System.NonSerialized] public CollisionFlags2D collisionFlags;

    // PlayerInput
    [HideInInspector] public PlayerInput playerInput;

    // State Machine
    private BaseState currentState;
    public IdleState Idle = new IdleState();
    public WalkState Walk = new WalkState();
    public JumpState Jump = new JumpState();
    public WallSlideState WallSlide = new WallSlideState();

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        animator.SetFloat("WalkSpeed", characterProfile.moveSpeed);
        currentState = Idle;
    }


    void Update()
    {
        MovementUpdate();
        currentState.UpdateState(this);
        Debug.Log($"Current State: {currentState.GetType().Name}");
    }

    void MovementUpdate()
    {
        Vector2 movement = Vector2.zero;

        // Utilise l'action Move du PlayerInput
        Vector2 moveInput = Vector2.zero;
        if (playerInput != null && playerInput.actions != null && playerInput.actions["Move"] != null)
        {
            moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        }
        movement.x = moveInput.x * characterProfile.moveSpeed * Time.deltaTime;

        // adapt graphics
        if (movement.x < 0)
        {
            graphicTransform.localScale = new Vector3(
                -Mathf.Abs(graphicTransform.localScale.x),
                graphicTransform.localScale.y,
                graphicTransform.localScale.z
            );
            graphicTransform.localPosition = new Vector3(
                -graphicMargin,
                0,
                0
            );
        }
        if (movement.x > 0)
        {
            graphicTransform.localScale = new Vector3(
                Mathf.Abs(graphicTransform.localScale.x),
                graphicTransform.localScale.y,
                graphicTransform.localScale.z
            );
            graphicTransform.localPosition = Vector3.zero;
        }

        if (playerInput.actions["Jump"].WasPressedThisFrame()) TryJump();

        float jumpMultiplier = 1;
        if (isJumping)
        {
            float timeSinceJumped = Time.time - jumpTimestamp;

            float yPositionCurrentFrame = characterProfile.gravityMultiplierCurve.Evaluate(timeSinceJumped);
            float yPositionPreviousFrame = characterProfile.gravityMultiplierCurve.Evaluate(timeSinceJumped - Time.deltaTime);
            movement.y = yPositionCurrentFrame - yPositionPreviousFrame;

            float xMax = characterProfile.gravityMultiplierCurve.keys[characterProfile.gravityMultiplierCurve.keys.Length - 1].time;
            if (timeSinceJumped > xMax)
            {
                isJumping = false;
            }
        }
        else
        {
            if (isWallSliding)
            {
                movement.y = -characterProfile.wallSlidingSpeed * Time.deltaTime;
            }
            else
            {
                movement.y = characterProfile.gravity * jumpMultiplier * -1 * Time.deltaTime;
            }
        }
        if (isUnderCoyoteTime)
            {
                float timeSinceFell = Time.time - coyoteTimestamp;
                if (timeSinceFell > characterProfile.maxCoyoteTime)
                {
                    remainingJumps--;
                    isUnderCoyoteTime = false;
                }
            }

        Move(movement);
    }

    public void TryJump()
    {

        if (remainingJumps < 1) return;
        animator.SetTrigger("Jump");

        isUnderCoyoteTime = false;
        isGrounded = false;
        isJumping = true;
        jumpTimestamp = Time.time;
        remainingJumps--;

        int jumpIndex = characterProfile.maxAllowedJumps - (remainingJumps + 1);
        onJumped?.Invoke(jumpIndex);
    }

    void Move(Vector2 movement)
    {
        bool collH = HorizontalMovement(movement.x);
        bool collV = VerticalMovement(movement.y);
        if (collH || collV) onCollisionStay?.Invoke(collisionFlags);
    }

    bool HorizontalMovement(float movement)
    {
        bool isThereCollision = raycaster.CalculateCollision(
            movement > 0 ? MovementDirection.Right : MovementDirection.Left,
            Mathf.Abs(movement)
        );

        if (isThereCollision)
        {
            if (movement > 0) collisionFlags |= CollisionFlags2D.Right;
            else collisionFlags |= CollisionFlags2D.Left;
            return true;
        }

        if (movement > 0) collisionFlags &= ~CollisionFlags2D.Right;
        else collisionFlags &= ~CollisionFlags2D.Left;
        self.Translate(Vector3.right * movement);
        return false;
    }

    bool VerticalMovement(float movement)
    {
        bool isThereCollision = raycaster.CalculateCollision(
            movement > 0 ? MovementDirection.Above : MovementDirection.Below,
            Mathf.Abs(movement)
        );

        if (isThereCollision)
        {
            if (movement < 0)
            {
                isGrounded = true;
                animator.SetBool("IsGrounded", true);
                isUnderCoyoteTime = false;
                remainingJumps = characterProfile.maxAllowedJumps;
                collisionFlags |= CollisionFlags2D.Below;
                onGrounded?.Invoke();
            }
            collisionFlags |= CollisionFlags2D.Above;
            return true;
        }
        if (movement < 0)
        {
            if (isGrounded)
            {
                isUnderCoyoteTime = true;
                coyoteTimestamp = Time.time;
                onFell?.Invoke();
            }

            collisionFlags &= ~CollisionFlags2D.Below;
            isGrounded = false;
            animator.SetBool("IsGrounded", false);
        }

        collisionFlags &= ~CollisionFlags2D.Above;

        self.Translate(Vector3.up * movement);

        return false;
    }

    // State Machine
    
    public void SwitchState(BaseState newState)
    {
        if (currentState != newState)
        {
            currentState.ExitState(this);
            currentState = newState;
            currentState.EnterState(this);
        }
    }
}