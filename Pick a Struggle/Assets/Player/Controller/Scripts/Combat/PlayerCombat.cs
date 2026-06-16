using PurrNet;
using UnityEngine;

public class PlayerCombat : NetworkIdentity
{
    private PlayerStatHandler _statHandler;
    private PlayerRagdoll _playerRagdoll;
    private PlayerState _playerState;

    // Health stats
    private float _health;
    [field: SerializeField] public float CurrentPlayerHealth { get; private set; }

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
    private bool _deathSequence;

    // Other
    [SerializeField] private bool _debug;

    [Header("Fall Damage Mult")]
    [SerializeField] private float _fallDamageMult = 1;

    // States
    public EPlayerCombatState CurrentPlayerCombatState { get; private set; } = EPlayerCombatState.emptyHanded;

    private void Awake()
    {
        // Components
        _statHandler = GetComponent<PlayerStatHandler>();
        _playerRagdoll = GetComponent<PlayerRagdoll>();
        _playerState = GetComponent<PlayerState>();

        // Inspector based values
        _deathSequence = false;
    }

    private void Start() {
        // Set starting values
        _health = _statHandler.Stats.health;
        CurrentPlayerHealth = _health;

        _meleeDamage = _statHandler.Stats.meleeDamage;
        _meleeRange = _statHandler.Stats.meleeRange;
        _handlingSpeed = _statHandler.Stats.handlingSpeed;

        // Call the setters
        SetHealth();
        SetMeleeDamage();
        SetMeleeRange();
        SetHandlingSpeed();
    }

    private void Update() {
        // Checking for dead
        if (_playerState.isDead && !_deathSequence){
            // Run the sequence once
            DeathSequence();
            _deathSequence = true;
        }
    }

    private void DeathSequence(){
        // All actions that happen with death
        Debug.Log("Player Has Died");
        _playerRagdoll.StunPlayer(Vector3.up, (1933/54));
    }

    // Player affects
    public void DealDamage(float dmg){
        CurrentPlayerHealth -= dmg;

        if(CurrentPlayerHealth <= 0){
            // Death logic
            _playerState.isDead = true;
            CurrentPlayerHealth = 0;
        }

        if(_debug)
            Debug.Log($"Player health took a hit for {dmg} HP");
    }

    public void FallDamage(Vector3 dir, float forceMult){
        if (_debug)
            Debug.Log("Fallen");

        // Deal damage
        DealDamage(Mathf.Abs(dir.y) * _fallDamageMult);

        // Ragdoll direction and logic
        Vector3 ragdollForce = new Vector3(dir.x, 0f, dir.z);
        _playerRagdoll.StunPlayer(ragdollForce, forceMult); // begin the ragdoll
    }

    // Setters used to ensure the stats are accurate to boosts
    void SetHealth() { _totalHealth = _health + _boostHealth; }
    void SetMeleeDamage() { _totalMeleeDamage = _meleeDamage + _boostMeleeDamage; }
    void SetMeleeRange() { _totalMeleeRange = _meleeRange + _boostMeleeRange; }
    void SetHandlingSpeed() { _totalHandlingSpeed = _handlingSpeed + _boostHandlingSpeed; }

    // Boosts for gaining boosts
    void BoostHealth(float boost){ _boostHealth += boost; }
    void BoostMeleeDamage(float boost){ _boostMeleeDamage += boost; }
    void BoostMeleeRange(float boost){ _boostMeleeRange += boost; }
    void BoostHandlingSpeed(float boost){ _boostHandlingSpeed += boost; }
}