using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public PlayerStats stats; 

    // Health stats
    float health;
    float currHealth;

    float boostHealth = 0;

    float totalHealth;

    // Combat stats
    float meleeDamage;
    float meleeRange;
    float handlingSpeed;

    float boostMeleeDamage = 0;
    float boostMeleeRange = 0;
    float boostHandlingSpeed = 0;

    float totalMeleeDamage;
    float totalMeleeRange;
    float totalHandlingSpeed;

    void Start() {
        // Set starting values
        health = stats.health;
        currHealth = health;

        meleeDamage = stats.meleeDamage;
        meleeRange = stats.meleeRange;
        handlingSpeed = stats.handlingSpeed;

        // Call the setters
        SetHealth();
        SetMeleeDamage();
        SetMeleeRange();
        SetHandlingSpeed();

        // Other set-up calls
    }

    void Update() {
        
    }

    // Setters used to ensure the stats are accurate to boosts
    void SetHealth() { totalHealth = health + boostHealth; }
    void SetMeleeDamage() { totalMeleeDamage = meleeDamage + boostMeleeDamage; }
    void SetMeleeRange() { totalMeleeRange = meleeRange + boostMeleeRange; }
    void SetHandlingSpeed() { totalHandlingSpeed = handlingSpeed + boostHandlingSpeed; }
}