using PurrNet;
using UnityEngine;

public class PlayerCombat : NetworkIdentity
{
    public PlayerStats stats;
    private RagdollLogic ragdoll;
    private PlayerAnimatior animator;

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

    // Other
    private Rigidbody rb;

    [SerializeField] private bool debug;

    [Header("Fall Damage Mult")]
    [SerializeField] private float fallDamageMult = 1;

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
        ragdoll = GetComponent<RagdollLogic>();
        animator = GetComponent<PlayerAnimatior>();
        rb = GetComponent<Rigidbody>();

        isDead = false;
        deathSequence = false;
    }

    private void Update() {
        if (isDead && !deathSequence){
            // Run the sequence once
            DeathSequence();
            deathSequence = true;
        }
    }

    private void DeathSequence(){
        // All actions that happen with death
        Debug.Log("Player Has Died");
        ragdoll.TossRagdoll(Vector3.up / rb.mass, (1933/54));
    }

    // Player affects
    public void DealDamage(float dmg){
        currHealth -= dmg;

        if(currHealth <= 0){
            // Death logic
            isDead = true;
            currHealth = 0;
        }

        if(debug)
            Debug.Log($"Player health took a hit for {dmg} HP");
    }

    public void FallDamage(Vector3 dir, float forceMult){
        // Deal damage
        DealDamage(dir.y * fallDamageMult);

        // Ragdoll direction and logic
        Vector3 ragdollForce = new Vector3(dir.x, 0, dir.z);
        animator.StunPlayer(currHealth != 0, ragdollForce, forceMult); // begin the ragdoll
    }

    // Setters used to ensure the stats are accurate to boosts
    void SetHealth() { totalHealth = health + boostHealth; }
    void SetMeleeDamage() { totalMeleeDamage = meleeDamage + boostMeleeDamage; }
    void SetMeleeRange() { totalMeleeRange = meleeRange + boostMeleeRange; }
    void SetHandlingSpeed() { totalHandlingSpeed = handlingSpeed + boostHandlingSpeed; }

    // Boosts for gaining boosts
    void BoostHealth(float boost){ boostHealth += boost; }
    void BoostMeleeDamage(float boost){ boostMeleeDamage += boost; }
    void BoostMeleeRange(float boost){ boostMeleeRange += boost; }
    void BoostHandlingSpeed(float boost){ boostHandlingSpeed += boost; }
}
