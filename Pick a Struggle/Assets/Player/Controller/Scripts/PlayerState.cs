using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [field: SerializeField] public EPlayerMovementState CurrentPlayerMovementState { get; private set; } = EPlayerMovementState.Idling;
    
    public void SetPlayerMovementState(EPlayerMovementState playerMovementState) {
        CurrentPlayerMovementState = playerMovementState;
    }

    public bool InGroundedState() {
        return  CurrentPlayerMovementState == EPlayerMovementState.Idling ||
                CurrentPlayerMovementState == EPlayerMovementState.Walking ||
                CurrentPlayerMovementState == EPlayerMovementState.Running ||
                CurrentPlayerMovementState == EPlayerMovementState.Sprinting;
    }
}

public enum EPlayerMovementState {
    Idling = 0,
    Walking = 1,
    Running = 2,
    Sprinting = 3,
    Jumping = 4,
    Falling = 5,
    Strafing = 6,
}