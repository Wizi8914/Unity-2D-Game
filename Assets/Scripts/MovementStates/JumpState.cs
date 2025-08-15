using UnityEngine;

public class JumpState : BaseState
{
    public override void EnterState(CharacterController2D controller)
    {
    }

    public override void ExitState(CharacterController2D controller)
    {
    }

    public override void UpdateState(CharacterController2D controller)
    {
        if (controller.isGrounded)
        {
            controller.wallJumpDirection = 0;

            if (controller.playerInput.actions["Move"].ReadValue<Vector2>().x != 0)
            {
                controller.SwitchState(controller.Walk);
            }
            else
            {
                controller.SwitchState(controller.Idle);
            }
            return;
        }
        else
        {
            bool leftCollision = controller.raycaster.CalculateCollision(MovementDirection.Left, controller.raycaster.skinWidth * 4);
            bool rightCollision = controller.raycaster.CalculateCollision(MovementDirection.Right, controller.raycaster.skinWidth * 4);

            if (leftCollision && (controller.wallJumpDirection == 1 || controller.wallJumpDirection == 0))
            {
                controller.SwitchState(controller.WallSlide);
            }
            else if (rightCollision && (controller.wallJumpDirection == -1 || controller.wallJumpDirection == 0))
            {
                controller.SwitchState(controller.WallSlide);
            }
        }
    }
}
