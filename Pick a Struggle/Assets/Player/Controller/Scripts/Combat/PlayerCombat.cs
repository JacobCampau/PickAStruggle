using PurrNet;
using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;
using UnityEngine.XR;
using static UnityEngine.Rendering.HableCurve;
using static UnityEngine.UI.Image;

public class PlayerCombat : NetworkIdentity
{
    private PlayerStatHandler _statHandler;
    private PlayerRagdoll _playerRagdoll;
    private PlayerState _playerState;
    private PlayerActionsInput _playerActionInput;
    private PlayerHotbarController _playerHotbarController;

    private LineRenderer _trajectoryLine;
    [Header("Helper Obj")]
    public GameObject clientHelper;

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
    public bool weaponAimed = false;

    [Header("Damage Mults")]
    [SerializeField] private float _fallDamageMult = 1;
    [SerializeField] private float _thrownDamageMult = 1;
    [SerializeField] private float _explosionDamageMult = 1;

    [Header("Hitbox controls")]
    [SerializeField] private float _hitboxBufferTimer = 0f;
    private float _maxTimer;
    private PlayerHitbox[] _hitboxes;
    public Collider[] allPlayerColliders;

    [Header("Throw Settings")]
    public float throwSpeed;
    public float upwardThrust;

    [Header("Projectile Prediction Settings")]
    [SerializeField, Tooltip("The marker will show where the projectile will hit")]
    Transform hitMarker; 
    [SerializeField, Range(10, 100), Tooltip("The maximum number of points the LineRenderer can have")]
    int maxPoints = 50;
    [SerializeField, Range(0.01f, 0.5f), Tooltip("The time increment used to calculate the trajectory")]
    float increment = 0.025f;
    [SerializeField, Range(1.05f, 2f), Tooltip("The raycast overlap between points in the trajectory, this is a multiplier of the length between points. 2 = twice as long")]
    float rayOverlap = 1.1f;

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
        allPlayerColliders = GetComponentsInChildren<Collider>();
        _playerHotbarController = GetComponent<PlayerHotbarController>();
        
        _trajectoryLine = clientHelper.GetComponent<LineRenderer>();
        SetTrajectoryVisbility(false);

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
        CheckForThrowableItem();

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
        if(_playerActionInput.AimPressed
            && _playerHotbarController.HotbarItems[_playerHotbarController.currentSlotSelection] != null
            && _playerHotbarController.HotbarItems[_playerHotbarController.currentSlotSelection].layer == LayerMask.NameToLayer("Shooting Objects")) {
            _mainCamera.Priority = 0;
            _aimCamera.Priority = 1;

            weaponAimed = true;

            activeCameraTransform = _aimCamera.transform;
        } else {
            _mainCamera.Priority = 1;
            _aimCamera.Priority = 0;

            weaponAimed = false;

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

    private void CheckForThrowableItem() {
        Vector3 adjustedDir = activeCameraTransform.forward;

        // Arm the throw
        if(_playerHotbarController.HotbarItems[_playerHotbarController.currentSlotSelection] != null) {
            if(_playerActionInput.AimPressed
                && _playerHotbarController.HotbarItems[_playerHotbarController.currentSlotSelection].layer == LayerMask.NameToLayer("Throwable Objects")
                && !_playerActionInput.ThrowCharged) {
                _playerActionInput.SetThrowCharged(true);
            }
        }

        // Dearm the throw
        if((_playerActionInput.ThrowCharged && !_playerActionInput.AimPressed) || _playerHotbarController.HotbarItems[_playerHotbarController.currentSlotSelection] == null)
            _playerActionInput.SetThrowCharged(false);

        // Throw control
        SetTrajectoryVisbility(_playerActionInput.ThrowCharged);

        if(_playerActionInput.ThrowCharged) {
            //Transform cam = activeCameraTransform;
            //RaycastHit hit;
            //if(Physics.Raycast(cam.position, cam.forward, out hit, 500f)) {
            //    //adjustedDir = (hit.point - _playerHotbarController.hand.position).normalized;
            //    //Debug.Log(hit.transform.name);
            //}

            PredictTrajectory(_playerHotbarController.HotbarItems[_playerHotbarController.currentSlotSelection], adjustedDir + (Vector3.up * upwardThrust));
        }

        if(_playerActionInput.ThrowCharged && _playerActionInput.ShootPressed){
            //Throw the damn thing
            _playerHotbarController.RequestThrow(throwSpeed, adjustedDir + (Vector3.up * upwardThrust));
        }
    }

    #region projectile prediction
    // yoinked projectile prediction code
    // commented out in facor of chat gpt rework
    //private void PredictTrajectory(Item projectile, Vector3 dir) {
    //    Vector3 velocity = dir.normalized * (throwSpeed / projectile.GetComponent<Rigidbody>().mass);
    //    Vector3 position = projectile.transformHandle.position;
    //    Vector3 nextPosition;
    //    float overlap;

    //    UpdateLineRender(maxPoints, (0, position));

    //    for(int i = 1; i < maxPoints; i++) {
    //        // Estimate velocity and update next predicted position
    //        velocity = CalculateNewVelocity(velocity, projectile.GetComponent<Rigidbody>().linearDamping, increment);
    //        nextPosition = position + velocity * increment;

    //        // Overlap our rays by small margin to ensure we never miss a surface
    //        overlap = Vector3.Distance(position, nextPosition) * rayOverlap;

    //        //When hitting a surface we want to show the surface marker and stop updating our line
    //        if(Physics.Raycast(position, velocity.normalized, out RaycastHit hit, overlap)) {
    //            UpdateLineRender(i, (i - 1, hit.point));
    //            MoveHitMarker(hit);
    //            break;
    //        }

    //        //If nothing is hit, continue rendering the arc without a visual marker
    //        hitMarker.gameObject.SetActive(false);
    //        position = nextPosition;
    //        UpdateLineRender(maxPoints, (i, position)); //Unneccesary to set count here, but not harmful
    //    }
    //}

    private void UpdateLineRender(int count, (int point, Vector3 pos) pointPos) {
        _trajectoryLine.positionCount = count;
        _trajectoryLine.SetPosition(pointPos.point, pointPos.pos);
    }

    private Vector3 CalculateNewVelocity(Vector3 velocity, float drag, float increment) {
        velocity += Physics.gravity * increment;
        //velocity *= Mathf.Clamp01(1f - drag * increment);
        return velocity;
    }

    private void MoveHitMarker(RaycastHit hit) {
        hitMarker.gameObject.SetActive(true);

        // Offset marker from surface
        float offset = 0.025f;
        hitMarker.position = hit.point + hit.normal * offset;
        hitMarker.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
    }

    private void SetTrajectoryVisbility(bool visible) {
        _trajectoryLine.enabled = visible;
    }
    #endregion

    // gpt reworded prediction
    private void PredictTrajectory(Item projectile, Vector3 dir) {
        Rigidbody rb = projectile.GetComponent<Rigidbody>();

        // Match the actual throw:
        // rb.AddForce(throwDir.normalized * force, ForceMode.Impulse);
        Vector3 velocity = dir.normalized * (throwSpeed / rb.mass);

        Vector3 position = projectile.transformHandle.position;

        // First point in the trajectory
        UpdateLineRender(maxPoints, (0, position));

        // Hide hit marker initially
        hitMarker.gameObject.SetActive(false);

        for(int i = 1; i < maxPoints; i++) {
            // Apply gravity and linear damping
            velocity = CalculateNewVelocity(
                velocity,
                rb.linearDamping,
                increment
            );

            // Calculate next predicted position
            Vector3 nextPosition = position + velocity * increment;

            // Direction and distance between this point and next point
            Vector3 segment = nextPosition - position;

            float rayDistance = segment.magnitude * rayOverlap;

            // Find every collider along this small trajectory segment
            RaycastHit[] hits = Physics.RaycastAll(
                position,
                segment.normalized,
                rayDistance
            );

            RaycastHit? closestValidHit = null;

            foreach(RaycastHit hit in hits) {
                // Ignore the current player's own colliders
                bool isOwnPlayerCollider = false;

                foreach(Collider playerCollider in allPlayerColliders) {
                    if(playerCollider == null)
                        continue;

                    if(hit.collider == playerCollider) {
                        isOwnPlayerCollider = true;
                        break;
                    }
                }

                if(isOwnPlayerCollider)
                    continue;

                // Ignore the projectile itself
                if(hit.collider.transform.IsChildOf(projectile.transform))
                    continue;

                // Find the closest valid hit
                if(!closestValidHit.HasValue ||
                    hit.distance < closestValidHit.Value.distance) {
                    closestValidHit = hit;
                }
            }

            // If we found something valid, stop the trajectory there
            if(closestValidHit.HasValue) {
                RaycastHit hit = closestValidHit.Value;

                UpdateLineRender(i + 1, (i, hit.point));
                MoveHitMarker(hit);

                break;
            }

            // Nothing hit, continue trajectory
            position = nextPosition;

            UpdateLineRender(
                maxPoints,
                (i, position)
            );
        }
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