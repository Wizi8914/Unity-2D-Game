using System.Collections;
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
    public TrailRenderer dashTrail;

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
    [System.NonSerialized] public int remainingDashes;
    [System.NonSerialized] public CollisionFlags2D collisionFlags;

    [System.NonSerialized] public bool canDash;
    [System.NonSerialized] public bool isDashing;
    [System.NonSerialized] public float dashCooldownTimer;
    [System.NonSerialized] public bool isDashOnCooldown;

    // References
    private HUDManager hudManager;



    // PlayerInput
    [HideInInspector] public PlayerInput playerInput;

    // State Machine
    private BaseState currentState;
    public IdleState Idle = new IdleState();
    public WalkState Walk = new WalkState();
    public JumpState Jump = new JumpState();
    public WallSlideState WallSlide = new WallSlideState();
    public DashingState Dashing = new DashingState();

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        animator.SetFloat("WalkSpeed", characterProfile.moveSpeed);
        canDash = true;
        remainingDashes = characterProfile.maxAllowedDashes;
        currentState = Idle;
        dashCooldownTimer = 0f;
        isDashOnCooldown = false;
        
        // Find HUD Manager reference
        hudManager = GameManager.Instance.canvas.GetComponentInChildren<HUDManager>();
    }


    void Update()
    {
        MovementUpdate();
        currentState.UpdateState(this);
        
        // Handle dash cooldown
        if (isDashOnCooldown)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                isDashOnCooldown = false;
                remainingDashes++;
                if (remainingDashes > characterProfile.maxAllowedDashes)
                {
                    remainingDashes = characterProfile.maxAllowedDashes;
                }
                
                // Update HUD
                if (hudManager != null)
                {
                    hudManager.UpdateHUD();
                }
                
                // If we still have more dashes to recharge, start next cooldown
                if (remainingDashes < characterProfile.maxAllowedDashes)
                {
                    StartNextDashCooldown();
                }
            }
            
            // Update dash cooldown slider
            if (hudManager != null)
            {
                float normalizedCooldown = dashCooldownTimer / characterProfile.dashingCooldown;
                hudManager.UpdateDashCooldown(normalizedCooldown);
            }
        }
        
        Debug.Log($"Current State: {currentState.GetType().Name}");
    }

    void MovementUpdate()
    {
        // Skip normal movement if dashing
        if (isDashing)
        {
            return;
        }

        Vector2 movement = Vector2.zero;

        // Use the Move action from PlayerInput
        Vector2 moveInput = Vector2.zero;
        if (playerInput != null && playerInput.actions != null && playerInput.actions["Move"] != null)
        {
            moveInput = playerInput.actions["Move"].ReadValue<Vector2>();
        }
        movement.x = moveInput.x * characterProfile.moveSpeed * Time.deltaTime;

        // Adapt graphics orientation (but not during wall sliding)
        if (!isWallSliding)
        {
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
        }

        // Jump handling

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
        
        // Coyote time handling
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

    public void Move(Vector2 movement)
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

    public IEnumerator ReloadDash()
    {
        yield return new WaitForSeconds(characterProfile.dashingCooldown);
        canDash = true;
        // Note: This method is kept for potential future use
        // Current dash system uses remainingDashes counter instead
    }

    public void StartNextDashCooldown()
    {
        isDashOnCooldown = true;
        dashCooldownTimer = characterProfile.dashingCooldown;
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