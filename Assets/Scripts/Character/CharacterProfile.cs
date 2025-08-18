using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "Simon/CharacterProfile")]
public class CharacterProfile : ScriptableObject
{
    [Header("Movement Parameters")]
    public float moveSpeed = 5;
    public float gravity = 14;
    public int maxAllowedJumps = 3;
    public float maxCoyoteTime = 0.3f;
    public AnimationCurve gravityMultiplierCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Header("Wall Jump Parameters")]
    public bool canWallJump = true;
    public float wallSlidingSpeed = 2f;
    public int wallJumpForce;
    [Header("Dash Parameters")]
    public float dashingPower;
    public float dashingTime;
    public float dashingCooldown;
    public int maxAllowedDashes = 1;
    public AnimationCurve dashAccelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}
