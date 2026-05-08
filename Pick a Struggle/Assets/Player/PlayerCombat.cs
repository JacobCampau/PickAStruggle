using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public PlayerStats stats; 

    // Health stats
    private float health;
    private float currHealth;

    private float boostHealth = 0;

    private float totalHealth;

    // Combat stats
    private float meleeDamage;
    private float meleeRange;
    private float handlingSpeed;

    private float boostMeleeDamage = 0;
    private float boostMeleeRange = 0;
    private float boostHandlingSpeed = 0;

    private float totalMeleeDamage;
    private float totalMeleeRange;
    private float totalHandlingSpeed;

    // Death info
    public bool isDead;
    private bool deathSequence;

    private void Start() {
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
        isDead = false;
        deathSequence = false;
    }

    private void Update() {
        if (isDead && !deathSequence){
            DeathSequence();
            deathSequence = true;
        }
    }

    private void DeathSequence(){
        // All actions that happen with death
    }

    // Player affects
    public void dealDamage(float dmg){
        currHealth -= dmg;

        if(currHealth <= 0){
            // Death logic
            isDead = true;
            currHealth = 0;
        }
    }

    // Setters used to ensure the stats are accurate to boosts
    void SetHealth() { totalHealth = health + boostHealth; }
    void SetMeleeDamage() { totalMeleeDamage = meleeDamage + boostMeleeDamage; }
    void SetMeleeRange() { totalMeleeRange = meleeRange + boostMeleeRange; }
    void SetHandlingSpeed() { totalHandlingSpeed = handlingSpeed + boostHandlingSpeed; }
}
