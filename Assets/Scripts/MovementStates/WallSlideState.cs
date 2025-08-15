using UnityEngine;

public class WallSlideState : BaseState
{

    public override void EnterState(CharacterController2D controller)
    {
        controller.animator.SetBool("IsWallSliding", true);
        controller.isJumping = false;
        controller.isWallSliding = true;

    }

    public override void ExitState(CharacterController2D controller)
    {
        controller.animator.SetBool("IsWallSliding", false);
        controller.isWallSliding = false;
    }

    public override void UpdateState(CharacterController2D controller)
    {
        if (controller.isGrounded)
        {
            controller.SwitchState(controller.Idle);
            return;
        }

        // Vérification des collisions avec les murs
        bool leftCollision = controller.raycaster.CalculateCollision(MovementDirection.Left, controller.raycaster.skinWidth * 4);
        bool rightCollision = controller.raycaster.CalculateCollision(MovementDirection.Right, controller.raycaster.skinWidth * 4);
    
        if (!leftCollision && !rightCollision)
        {
            controller.SwitchState(controller.Jump);
            return;
        }
        else
        {
            controller.wallJumpDirection = leftCollision ? -1 : 1;

            if (controller.playerInput.actions["Jump"].WasPressedThisFrame())
            {
                if (controller.characterProfile.canWallJump)
                {
                    controller.isWallSliding = false;
                    controller.wallJumpDirection = leftCollision ? -1 : 1;
                    controller.remainingJumps++;
                    Vector2 jumpDirection = new Vector2(controller.wallJumpDirection * controller.characterProfile.wallJumpForce, controller.characterProfile.wallJumpForce);
                    controller.self.GetComponent<Rigidbody2D>().AddForce(jumpDirection, ForceMode2D.Impulse);
                    controller.SwitchState(controller.Jump);
                }
                return;
            }
        }
    }
}
