using PurrNet;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerCombat : NetworkIdentity
{
    private PlayerStatHandler _statHandler;
    private PlayerRagdoll _playerRagdoll;
    private PlayerState _playerState;
    private PlayerActionsInput _playerActionInput;

    // Health stats
    private float _health;
    [field: SerializeField] public float CurrentPlayerHealth { get; private set; }

    private float _boostHealth = 0;

    [field: SerializeField] public float TotalHealth { get; private set; }

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
    [SerializeField] private bool _deathSequence = false;

    // Other
    [SerializeField] private bool _debug;
    public bool _hitboxDebug;

    [Header("Camera")]
    [SerializeField] private CinemachineCamera _mainCamera;
    [SerializeField] private CinemachineCamera _aimCamera;
    public Transform activeCameraTransform;

    [Header("Damage Mults")]
    [SerializeField] private float _fallDamageMult = 1;
    [SerializeField] private float _thrownDamageMult = 1;
    [SerializeField] private float _explosionDamageMult = 1;

    [Header("Hitbox controls")]
    [SerializeField] private float _hitboxBufferTimer = 0f;
    private float _maxTimer;
    private PlayerHitbox[] _hitboxes;

    // States
    public EPlayerCombatState CurrentPlayerCombatState { get; private set; } = EPlayerCombatState.emptyHanded;

    private void Awake()
    {
        // Components
        _playerActionInput = GetComponent<PlayerActionsInput>();
        _statHandler = GetComponent<PlayerStatHandler>();
        _playerRagdoll = GetComponent<PlayerRagdoll>();
        _playerState = GetComponent<PlayerState>();
        _hitboxes = GetComponentsInChildren<PlayerHitbox>();

        // Values
        _maxTimer = _hitboxBufferTimer;
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
        if (_playerState.IsDead && !_deathSequence){
            // Run the sequence once
            DeathSequence();
        }

        if(_playerState.IsDead) return;

        // Aim Logic
        SetActiveCamera();

        // Hitbox timer
        if(!_playerState.IsActiveHitbox) {
            _hitboxBufferTimer -= Time.deltaTime;
            if(_hitboxBufferTimer < 0) {
                _playerState.SetIsActiveHitbox(true);
            }
        }
    }

    private void DeathSequence(){
        // All actions that happen with death
        _deathSequence = true;

        // stun if not done already
        if(_playerState.CurrentRagdollState == ERagdollState.Complete) {
            _playerRagdoll.Stun(Vector3.up, (1933/54), null);
        }
    }

    void SetActiveCamera() {
        if(_playerActionInput.AimPressed) {
            _mainCamera.Priority = 0;
            _aimCamera.Priority = 1;

            activeCameraTransform = _aimCamera.transform;
        } else {
            _mainCamera.Priority = 1;
            _aimCamera.Priority = 0;

            activeCameraTransform = _mainCamera.transform;
        }
    }

    // Player affects
    public void DealDamage(float dmg){
        if(_playerState.IsDead) return;

        CurrentPlayerHealth -= dmg;

        if(CurrentPlayerHealth <= 0.01) {
            // Death logic
            _playerState.SetIsDead(true);
            CurrentPlayerHealth = 0;
        }

        _playerState.SetIsActiveHitbox(false);
        _hitboxBufferTimer = _maxTimer;

        if(_debug)
            Debug.Log($"Player health took a hit for {dmg} HP");
    }

    public void FallDamage(Vector3 dir){
        if (_debug)
            Debug.Log("Fallen");

        // Deal damage
        DealDamage(Mathf.Abs(dir.y) * _fallDamageMult);

        // Ragdoll direction and logic
        Vector3 ragdollForce = new Vector3(dir.x, 0f, dir.z);
        _playerRagdoll.Stun(ragdollForce, 1, null); // break player
    }

    public void ThrownObjectDamage(Vector3 force, float mult, GameObject bone) {
        DealDamage(mult * _explosionDamageMult);
        _playerRagdoll.Stun(force, mult, bone);
    }

    public void ExplosionDamage(Vector3 force, float mult, GameObject bone) {
        DealDamage(mult * _explosionDamageMult);
        _playerRagdoll.Stun(force, mult, bone);
    }

    // Setters used to ensure the stats are accurate to boosts
    void SetHealth() { TotalHealth = _health + _boostHealth; }
    void SetMeleeDamage() { _totalMeleeDamage = _meleeDamage + _boostMeleeDamage; }
    void SetMeleeRange() { _totalMeleeRange = _meleeRange + _boostMeleeRange; }
    void SetHandlingSpeed() { _totalHandlingSpeed = _handlingSpeed + _boostHandlingSpeed; }

    // Boosts for gaining boosts
    void BoostHealth(float boost){ _boostHealth += boost; }
    void BoostMeleeDamage(float boost){ _boostMeleeDamage += boost; }
    void BoostMeleeRange(float boost){ _boostMeleeRange += boost; }
    void BoostHandlingSpeed(float boost){ _boostHandlingSpeed += boost; }
}