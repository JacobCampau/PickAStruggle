using TMPro.EditorUtilities;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerStateMachine : StateMachine<PlayerStateMachine.EPlayerState>
{
    public enum EPlayerState { 
        Moving,
        Ragdoll,
        Animated
    }

    private PlayerContext _context;

    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private BoxCollider _playerCollider;

    private void Awake(){
        _context = new PlayerContext(_playerStats, _rigidbody, _playerCollider);
        InitializeStates();
    }

    private void InitializeStates()
    {
        states.Add(EPlayerState.Moving, new PlayerMove(_context, EPlayerState.Moving));
        currState = states[EPlayerState.Moving];
    }
}
