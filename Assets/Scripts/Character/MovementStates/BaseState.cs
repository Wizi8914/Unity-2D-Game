public abstract class BaseState
{
    public abstract void EnterState(CharacterController2D controller);

    public abstract void UpdateState(CharacterController2D controller);

    public abstract void ExitState(CharacterController2D controller);
}
