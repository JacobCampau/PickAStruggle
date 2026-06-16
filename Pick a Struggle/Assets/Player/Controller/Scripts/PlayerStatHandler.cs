using UnityEngine;

public class PlayerStatHandler : MonoBehaviour
{
    [Header("Struggle Settings")]
    [SerializeField] private PlayerStats _playerStats;

    public PlayerStats Stats => _playerStats;
}
