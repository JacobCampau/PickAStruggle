using UnityEngine;

public abstract class PlayerState : State<PlayerStateMachine.EPlayerState>
{
    protected PlayerContext Context;

    public PlayerState(PlayerContext context, PlayerStateMachine.EPlayerState stateKey) : base(stateKey)
    {
        Context = context;
    }
}
