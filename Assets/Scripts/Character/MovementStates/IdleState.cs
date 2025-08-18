using UnityEngine;

public class IdleState : BaseState
{
    public override void EnterState(CharacterController2D controller)
    {
    }

    public override void ExitState(CharacterController2D controller)
    {
    }

    public override void UpdateState(CharacterController2D controller)
    {
        if (controller.playerInput.actions["Move"].ReadValue<Vector2>().x != 0)
        {
            controller.SwitchState(controller.Walk);
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
