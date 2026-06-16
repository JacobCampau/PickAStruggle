using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    /*========== Info For All Players ==========*/
    [Header("Health")]
    public float health = 100;

    [Header("Movement")]
    public float crouchSpeed = 10;
    public float runSpeed = 10;
    public float sprintSpeed = 10;
    [Space]
    public float staminaMax = 100;
    public float staminaDrain = 5;
    [Space]
    public float jumpForce = 5;

    [Header("Combat")]
    public float meleeDamage = 10;
    public float meleeRange = 1;
    public float handlingSpeed = 1;

    [Header("Emotion")]
    public float emotionMax = 100;
    public float emotionBuildup = 1;
}
