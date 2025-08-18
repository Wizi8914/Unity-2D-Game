using UnityEngine;

public class WalkState : BaseState
{
    public override void EnterState(CharacterController2D controller)
    {
        // Set the animator parameter for walking
        controller.animator.SetBool("IsMovingHorizontally", true);
    }

    public override void ExitState(CharacterController2D controller)
    {
        controller.animator.SetBool("IsMovingHorizontally", false);
    }

    public override void UpdateState(CharacterController2D controller)
    {
        if (controller.playerInput.actions["Move"].ReadValue<Vector2>().x == 0)
        {
            controller.SwitchState(controller.Idle);
        }

        if (controller.playerInput.actions["Jump"].WasPressedThisFrame())
        {
            controller.SwitchState(controller.Jump);
        }

        if (controller.playerInput.actions["Dash"].WasPressedThisFrame() && controller.remainingDashes > 0)
        {
            controller.SwitchState(controller.Dashing);
        }
    }
}
