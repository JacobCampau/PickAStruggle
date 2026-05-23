using UnityEngine;

public class PlayerMove : PlayerState
{
    public PlayerMove(PlayerContext context, PlayerStateMachine.EPlayerState estate) : base(context, estate) {
        PlayerContext Context = context;
    }

    public override void EnterState() { 
    
    }
    
    public override void ExitState() { 
    
    }
    
    public override void UpdateState() { 
    
    }
    
    public override PlayerStateMachine.EPlayerState GetNextState() {
        return stateKey;
    }
}
