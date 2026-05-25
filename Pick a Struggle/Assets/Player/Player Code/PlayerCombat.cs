using PurrNet;
using UnityEngine;

public class PlayerCombat : NetworkIdentity
{
    private PlayerHandler _handler;

    private RagdollLogic _ragdoll;
    private PlayerAnimator _animator;

    // Health stats
    private float _health;
    private float _currHealth;

    private float _boostHealth = 0;

    private float _totalHealth;

    // Combat stats
    private float _meleeDamage;
    private float _meleeRange;
    private float _handlingSpeed;

    private float _boostMeleeDamage = 0;
    private float _boostMeleeRange = 0;
    private float _boostHandlingSpeed = 0;

    private float _totalMeleeDamage;
    private float _totalMeleeRange;
    private float _totalHandlingSpeed;

    // Death info
    public bool isDead;
    private bool _deathSequence;

    // Other
    [SerializeField] private bool _debug;

    [Header("Fall Damage Mult")]
    [SerializeField] private float _fallDamageMult = 1;

    // States
    public enum EPlayerCombatState
    {
        emptyHanded,
        oneHanded,
        twoHanded
    }
    public EPlayerCombatState combatState;

    private void Awake()
    {
        // Components
        _handler = GetComponent<PlayerHandler>();
        _ragdoll = GetComponent<RagdollLogic>();
        _animator = GetComponent<PlayerAnimator>();

        // Inspector based values
        isDead = false;
        _deathSequence = false;
    }

    private void Start() {
        // Set starting values
        _health = _handler.Stats.health;
        _currHealth = _health;

        _meleeDamage = _handler.Stats.meleeDamage;
        _meleeRange = _handler.Stats.meleeRange;
        _handlingSpeed = _handler.Stats.handlingSpeed;

        // Call the setters
        SetHealth();
        SetMeleeDamage();
        SetMeleeRange();
        SetHandlingSpeed();
    }

    private void Update() {
        // Checking for dead
        if (isDead && !_deathSequence){
            // Run the sequence once
            DeathSequence();
            _deathSequence = true;
        }
    }

    private void DeathSequence(){
        // All actions that happen with death
        Debug.Log("Player Has Died");
        _ragdoll.TossRagdoll(Vector3.up / _handler.RB.mass, (1933/54));
    }

    // Player affects
    public void DealDamage(float dmg){
        _currHealth -= dmg;

        if(_currHealth <= 0){
            // Death logic
            isDead = true;
            _currHealth = 0;
        }

        if(_debug)
            Debug.Log($"Player health took a hit for {dmg} HP");
    }

    public void FallDamage(Vector3 dir, float forceMult){
        if (_debug)
            Debug.Log("Fallen");

        // Deal damage
        DealDamage(dir.y * _fallDamageMult);

        // Ragdoll direction and logic
        Vector3 ragdollForce = new Vector3(dir.x, 0, dir.z);
        _animator.StunPlayer(ragdollForce, forceMult); // begin the ragdoll
    }

    // Setters used to ensure the stats are accurate to boosts
    void SetHealth() { _totalHealth = _health + _boostHealth; }
    void SetMeleeDamage() { _totalMeleeDamage = _meleeDamage + _boostMeleeDamage; }
    void SetMeleeRange() { _totalMeleeRange = _meleeRange + _boostMeleeRange; }
    void SetHandlingSpeed() { _totalHandlingSpeed = _handlingSpeed + _boostHandlingSpeed; }

    // Boosts for gaining boosts
    void BoostHealth(float boost){ _boostHealth += _boost; }
    void BoostMeleeDamage(float boost){ _boostMeleeDamage += _boost; }
    void BoostMeleeRange(float boost){ _boostMeleeRange += _boost; }
    void BoostHandlingSpeed(float boost){ _boostHandlingSpeed += _boost; }
}
