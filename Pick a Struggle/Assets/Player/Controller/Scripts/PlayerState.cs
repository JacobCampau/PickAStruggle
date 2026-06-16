using UnityEngine;

public class PlayerState : MonoBehaviour
{
    [field: SerializeField] public EPlayerMovementState CurrentPlayerMovementState { get; private set; } = EPlayerMovementState.Idling;
    [field: SerializeField] public EHeadTrackingState CurrentPlayerTrackingState { get; private set; } = EHeadTrackingState.Default;
    [field: SerializeField] public ERagdollState CurrentRagdollState { get; private set; } = ERagdollState.Complete;

    public bool isDead = false;
    
    public void SetPlayerMovementState(EPlayerMovementState playerMovementState) {
        CurrentPlayerMovementState = playerMovementState;
    }

    public void SetPlayerRagdollState(ERagdollState ragdollState) {
        CurrentRagdollState = ragdollState;
    }

    public void SetPlayerTrackingState(EHeadTrackingState headTrackState) {
        CurrentPlayerTrackingState = headTrackState;
    }

    public bool InGroundedState() {
        return IsStateGroundedState(CurrentPlayerMovementState);
    }

    public bool IsStateGroundedState(EPlayerMovementState movementState) {
        return movementState == EPlayerMovementState.Idling ||
                movementState == EPlayerMovementState.Crouching ||
                movementState == EPlayerMovementState.Running ||
                movementState == EPlayerMovementState.Sprinting;
    }
}

public enum EPlayerMovementState {
    Idling = 0,
    Crouching = 1,
    Running = 2,
    Sprinting = 3,
    Jumping = 4,
    Falling = 5,
    Ragdoll = 6,
}

public enum ERagdollState {
    Active = 0,
    StandingUp = 1,
    ResetingBones = 2,
    Complete = 3,
}

public enum EPlayerCombatState {
    emptyHanded = 0,
    oneHanded = 1,
    twoHanded = 2,
}