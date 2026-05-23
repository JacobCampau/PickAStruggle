using UnityEngine;
using PurrNet;

public class PlayerHandler : NetworkIdentity
{
    // Player stats
    [Header("Struggle Settings")]
    [SerializeField] private PlayerStats _playerStats;

    // Components of the player
    private Rigidbody _rigidbody;
    private BoxCollider _boxCollider;

    // States for the player
    public enum EPlayerState
    {
        moving,
        animated,
        ragdoll,
        dead
    }

    [Header("States")]
    public EPlayerState playerState;

    private void Awake()
    {
        // Components
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    // Getters
    public PlayerStats Stats => _playerStats;
    public Rigidbody RB => _rigidbody;
    public BoxCollider BoxCollider => _boxCollider;
}