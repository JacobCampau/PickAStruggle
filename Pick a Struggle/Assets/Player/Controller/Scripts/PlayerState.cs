using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [field: SerializeField] public EPlayerMovementState CurrentPlayerMovementState { get; private set; } = EPlayerMovementState.Idling;
    
    public void SetPlayerMovementState(EPlayerMovementState playerMovementState) {
        CurrentPlayerMovementState = playerMovementState;
    }

    public bool InGroundedState() {
        return IsStateGroundedState(CurrentPlayerMovementState);
    }

    public bool IsStateGroundedState(EPlayerMovementState movementState) {
        return movementState == EPlayerMovementState.Idling ||
                movementState == EPlayerMovementState.Walking ||
                movementState == EPlayerMovementState.Running ||
                movementState == EPlayerMovementState.Sprinting;
    }
}

public enum EPlayerMovementState {
    Idling = 0,
    Walking = 1,
    Running = 2,
    Sprinting = 3,
    Jumping = 4,
    Falling = 5,
    Ragdoll = 6,
}