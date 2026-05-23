using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerContext : MonoBehaviour
{
    // Information required across the player
    private PlayerStats _playerStats;
    private Rigidbody _rigidBody;
    private BoxCollider _playerCollider;

    // Initialize
    public PlayerContext(PlayerStats playerStats, Rigidbody rigidbody, BoxCollider playerCollider)
    {
        _playerStats = playerStats;
        _rigidBody = rigidbody;
        _playerCollider = playerCollider;
    }

    // public getter methods
    public Rigidbody RB => _rigidBody;
    public BoxCollider PlayerCollider => _playerCollider;
}
