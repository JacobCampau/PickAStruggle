using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using PurrNet;

public abstract class StateMachine<EState> : NetworkIdentity where EState : Enum
{
    protected Dictionary<EState, State<EState>> states = new Dictionary<EState, State<EState>>();
    protected State<EState> currState;
    private bool isTransitioning = false;

    private void Start(){
        currState.EnterState();
    }

    private void Update() {
        EState nextStateKey = currState.GetNextState();

        if(!isTransitioning && nextStateKey.Equals(currState.stateKey)) { 
            currState.UpdateState();
        }
        else if(!isTransitioning){
            TransitionToState(nextStateKey);
        }
    }

    private void TransitionToState(EState key){
        isTransitioning = true;
        currState.ExitState();
        currState = states[key];
        currState.EnterState();
        isTransitioning = false;
    }
}
