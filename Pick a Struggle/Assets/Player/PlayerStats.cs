using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Scriptable Objects/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    /*========== Info For All Players ==========*/
    // Health
    public float health = 100;

    // Movement
    public float speed = 10;
    public float staminaMax = 100;
    public float staminaDrain = 5;
    public float jumpForce = 5;

    // Combat
    public float meleeDamage = 10;
    public float meleeRange = 1;
    public float handlingSpeed = 1;

    // Emotion
    public float emotionMax;
    public float currEmotion;
    public float emotionBuildup;
    public float totalEmotionBuildup;

    // Boost
    public float boostEmotionBuildup;
}
