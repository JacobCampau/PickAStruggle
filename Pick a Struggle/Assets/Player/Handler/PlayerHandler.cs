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
    public EPlayerState playerState;

    private void Awake()
    {
        // Components
        _rigidbody = GetComponent<Rigidbody>();
        _boxCollider = GetComponent<BoxCollider>();
    }

    // Collisions onto the whole player --> temp to be changed
    private void OnCollisionEnter(Collider other){
        if(other.gameObject.tag == "Projectile"){
            GetComponent<PlayerAnimator>().StunPlayer(other.gameObject.GetComponent<Rigidbody>().linearVelocity, 5);
        }
    }

    // Getters
    public PlayerStats Stats => _playerStats;
    public Rigidbody RB => _rigidbody;
    public BoxCollider BoxCollider => _boxCollider;
}