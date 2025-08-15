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
    [System.NonSerialized] public bool isUnderCoyoteTime;
    [System.NonSerialized] public float jumpTimestamp;
    [System.NonSerialized] public float coyoteTimestamp;
    [System.NonSerialized] public int remainingJumps;
    [System.NonSerialized] public CollisionFlags2D collisionFlags;

    // PlayerInput
    public PlayerInput playerInput;

    // State System
    public BaseState currentState;
    public IdleState idleState = new IdleState();
    public WalkState walkState = new WalkState();
    public AirState airState = new AirState();
    public WallSlideState wallSlideState = new WallSlideState();

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        animator.SetFloat("WalkSpeed", characterProfile.moveSpeed);
                
        // Start in idle state
        currentState = idleState;
        currentState.EnterState(this);
    }

    void Update()
    {
        currentState.UpdateState(this);
    }

    public void ChangeState(BaseState newState)
    {
        if (currentState != newState)
        {
            currentState.ExitState(this);
            currentState = newState;
            currentState.EnterState(this);
        }
        movement.x = moveInput.x * characterProfile.moveSpeed * Time.deltaTime;

        // adapt graphics
        animator.SetBool("IsMovingHorizontally", movement.x != 0);
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
        if (movementX > 0)
        {
            graphicTransform.localScale = new Vector3(
                Mathf.Abs(graphicTransform.localScale.x),
                graphicTransform.localScale.y,
                graphicTransform.localScale.z
            );
            graphicTransform.localPosition = Vector3.zero;
        }
    }

    public bool IsAgainstWall()
        {
        return (collisionFlags & CollisionFlags2D.Left) != 0 || (collisionFlags & CollisionFlags2D.Right) != 0;
    }
    
    public void TryJump()
    {
        
        if (remainingJumps < 1) return;

        isUnderCoyoteTime = false;
        isGrounded = false;
        isJumping = true;
        jumpTimestamp = Time.time;
        remainingJumps--;

        int jumpIndex = characterProfile.maxAllowedJumps - (remainingJumps+1);
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
}
