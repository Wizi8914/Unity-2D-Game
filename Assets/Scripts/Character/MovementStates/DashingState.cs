using UnityEngine;
using System.Collections;

public class DashingState : BaseState
{
    private float dashStartTime;
    private Vector2 dashDirection;
    private bool dashCompleted;
    private float originalGravity;
    private float originalGravityScale;

    public override void EnterState(CharacterController2D controller)
    {
        controller.isDashing = true;
        controller.canDash = false;
        controller.remainingDashes--; // Consume one dash
        controller.animator.SetTrigger("Dash");

        // Start dash cooldown if needed
        if (controller.remainingDashes < controller.characterProfile.maxAllowedDashes && !controller.isDashOnCooldown)
        {
            controller.StartNextDashCooldown();
        }

        // Update HUD immediately
        HUDManager hudManager = Object.FindFirstObjectByType<HUDManager>();
        if (hudManager != null)
        {
            hudManager.UpdateHUD();
        }

        dashStartTime = Time.time;
        dashCompleted = false;

        Vector2 inputDirection = Vector2.zero;
        if (controller.playerInput != null && controller.playerInput.actions != null && controller.playerInput.actions["Move"] != null)
        {
            inputDirection = controller.playerInput.actions["Move"].ReadValue<Vector2>();
        }
        
        if (inputDirection.magnitude < 0.1f)
        {
            float facingDirection = Mathf.Sign(controller.graphicTransform.localScale.x);
            dashDirection = new Vector2(facingDirection, 0).normalized;
        }
        else
        {
            dashDirection = GetNearestEightDirection(inputDirection).normalized;
        }

        originalGravity = controller.characterProfile.gravity;
        controller.characterProfile.gravity = 0;
        
        Rigidbody2D rb = controller.self.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            originalGravityScale = rb.gravityScale;
            rb.gravityScale = 0;
        }

        // Enable dash trail effect
        controller.dashTrail.emitting = true;
    }

    public override void UpdateState(CharacterController2D controller)
    {
        float timeSinceDashStart = Time.time - dashStartTime;

        // Check if dash is completed
        if (timeSinceDashStart >= controller.characterProfile.dashingTime)
        {
            if (!dashCompleted)
            {
                dashCompleted = true;
                FinalizeDash(controller);
            }
            return;
        }

        // Calculate dash movement using acceleration curve
        float dashProgress = timeSinceDashStart / controller.characterProfile.dashingTime;
        float accelerationMultiplier = controller.characterProfile.dashAccelerationCurve.Evaluate(dashProgress);
        
        Vector2 dashMovement = dashDirection * controller.characterProfile.dashingPower * accelerationMultiplier * Time.deltaTime;
        controller.Move(dashMovement);
        
    }

    public override void ExitState(CharacterController2D controller)
    {
        controller.isDashing = false;
        controller.characterProfile.gravity = originalGravity;
        controller.dashTrail.emitting = false;
    }

    private void FinalizeDash(CharacterController2D controller)
    {
        if (controller.isGrounded)
        {
            if (Mathf.Abs(controller.playerInput.actions["Move"].ReadValue<Vector2>().x) > 0.1f)
            {
                controller.SwitchState(controller.Walk);
            }
            else
            {
                controller.SwitchState(controller.Idle);
            }
            controller.GetComponentInChildren<SoundEffect>().PlayLandSound();
        }
        else
        {
            if (controller.isJumping)
            {
                controller.SwitchState(controller.Jump);
            }
            else if (controller.isWallSliding)
            {
                controller.SwitchState(controller.WallSlide);
            }
            else
            {
                controller.SwitchState(controller.Jump);
            }
        }
    }
    
    private Vector2 GetNearestEightDirection(Vector2 inputDirection)
    {
        // Define the 8 possible directions
        Vector2[] eightDirections = {
            Vector2.right,              // Right (1, 0)
            new Vector2(1, 1),          // Up-Right (1, 1)
            Vector2.up,                 // Up (0, 1)
            new Vector2(-1, 1),         // Up-Left (-1, 1)
            Vector2.left,               // Left (-1, 0)
            new Vector2(-1, -1),        // Down-Left (-1, -1)
            Vector2.down,               // Down (0, -1)
            new Vector2(1, -1)          // Down-Right (1, -1)
        };
        
        // Find the direction with the smallest angle to the input
        Vector2 bestDirection = Vector2.right;
        float bestDot = -2f;
        
        foreach (Vector2 direction in eightDirections)
        {
            float dot = Vector2.Dot(inputDirection.normalized, direction.normalized);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestDirection = direction;
            }
        }
        
        return bestDirection;
    }
}