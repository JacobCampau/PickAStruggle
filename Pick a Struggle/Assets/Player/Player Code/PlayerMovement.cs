using UnityEngine;
using PurrNet;

public class PlayerMovement : NetworkIdentity
{
    // Debug
    [SerializeField] private bool _debug;

    private PlayerHandler _handler;

    private PlayerCombat _combat;

    // Move stats from stats
    private float _moveSpeed;
    private float _staminaMax = 100;
    private float _staminaDrain = 5;
    private float _jumpForce;

    // Trackers
    private float _currStamina;

    // Boosts and totals
    private float _boostSpeed;
    private float _boostStaminaDrain;

    private float _totalSpeed;
    private float _totalStaminaDrain;

    // Move stats from inspector
    [Header("Movement Tweaks")]
    [SerializeField] private float _sprintSpeedMult = 1.25f;
    [SerializeField] private float _crouchSpeedMult = 0.5f;
    [SerializeField] private float _groundDrag;
    [SerializeField] private float _jumpCooldown;
    [SerializeField] private float _airMult;
    [SerializeField] private float _groundMult;
    private bool _readyToJump;

    [Header("Key Binds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    [SerializeField] private LayerMask _whatIsGround;
    private bool _grounded;
    [SerializeField] private float _groundCheckDistance;

    [Header("Slope Handling")]
    [SerializeField] private float _maxSlopeAngle = 45f;
    private RaycastHit _slopeHit;
    private bool _exitingSlope;

    [Header("Fall Damage Limit")]
    [SerializeField] private float _maxVelocity;

    [Header("Other")]
    [SerializeField] private Transform _orientation;

    private float _horizontalInput;
    private float _verticalInput;

    private Vector3 _moveDir;

    // Move states
    public enum EPlayerMoveState
    {
        walking,
        sprinting,
        crouching,
        air
    }
    public EPlayerMoveState moveState;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;
    }

    private void Awake()
    {
        // Get the handler and other components
        _handler = GetComponent<PlayerHandler>();
        _combat = GetComponent<PlayerCombat>();

        // Initial booleans
        _readyToJump = true;
    }

    private void Start()
    {
        // Set starting values
        _moveSpeed = _handler.Stats.speed;
        _staminaMax = _handler.Stats.staminaMax;
        _staminaDrain = _handler.Stats.staminaDrain;
        _currStamina = _handler.Stats.staminaMax;
        _jumpForce = _handler.Stats.jumpForce;

        // Call the setters
        SetMoveSpeed();
        SetStaminaDrain();
    }

    private void Update() {
        // Always running, no matter frames
        StateHandler();
        SetStaminaDrain();

        // Ground check
        _grounded = Physics.Raycast(transform.position, Vector3.down, _groundCheckDistance, _whatIsGround);
        if(_debug)
            if(_grounded) {
                Debug.DrawRay(transform.position, Vector3.down * _groundCheckDistance, Color.green);
            } else { 
                Debug.DrawRay(transform.position, Vector3.down * _groundCheckDistance, Color.red);
            }

        // Jump
        if(Input.GetKey(jumpKey) && _grounded && _readyToJump) {
            _readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), _jumpCooldown);
        }

        // Drag
        if(_grounded) {
            _handler.RB.linearDamping = _groundDrag;
        } else {
            _handler.RB.linearDamping = 0;
        }

        // Stamina Drain
        if(moveState == EPlayerMoveState.sprinting){
            // Stamina drain
            _currStamina -= _totalStaminaDrain * Time.deltaTime;
            if(_currStamina <= 0){
                _currStamina = 0;
            }
        }else{
            // Regen stamina at twice the pace
            if(_currStamina < _staminaMax)
                _currStamina += _totalStaminaDrain * 2 * Time.deltaTime;
            
            if(_currStamina > _staminaMax)
            {
                _currStamina = _staminaMax;
            }
        }

        // Hitting the ground too hard
        if (FallingFast() && _grounded && _handler.playerState == PlayerHandler.EPlayerState.moving)
        {
            if (_debug)
                Debug.Log($"Player hit the ground too hard with velocity of {_handler.RB.linearVelocity.y}");
            _combat.FallDamage(_handler.RB.linearVelocity, 3);
        }
    }

    private void FixedUpdate() {
        if(_handler.playerState != PlayerHandler.EPlayerState.moving) return;
        
        // Movement
        MovePlayer();
    }

    private Vector3 GetMovementDir() {
        _horizontalInput = Input.GetAxisRaw("Horizontal");
        _verticalInput = Input.GetAxisRaw("Vertical");

        return _orientation.forward * _verticalInput + _orientation.right * _horizontalInput;
    }

    private void StateHandler() {
        if (_grounded && Input.GetKey(crouchKey)) {
            // Crouching
            moveState = EPlayerMoveState.crouching;
            SetCrouchSpeed();
        } else if (_grounded && Input.GetKey(sprintKey) && _currStamina > 0) {
            // Sprinting
            moveState = EPlayerMoveState.sprinting;
            SetSprintSpeed();
        } else if (_grounded) {
            // Regular moving or idle
            moveState = EPlayerMoveState.walking;
            SetMoveSpeed();
        } else {
            // Air
            moveState = EPlayerMoveState.air;
        }
    }

    private void MovePlayer() {
        // calc move direction
        _moveDir = GetMovementDir();

        // On slope
        if(OnSlope() && !_exitingSlope) {
            _handler.RB.AddForce(GetSlopeMoveDirection() * _totalSpeed * _groundMult, ForceMode.Force);

            if(_handler.RB.linearVelocity.y > 0)
                _handler.RB.AddForce(Vector3.down * 80f, ForceMode.Force);
        } else {
            if(moveState == EPlayerMoveState.air) {
                _handler.RB.AddForce(_moveDir.normalized * _totalSpeed * _airMult, ForceMode.Force);
            } else {
                _handler.RB.AddForce(_moveDir.normalized * _totalSpeed * _groundMult, ForceMode.Force);
            }
        }

        // Clamp speeds as needed
        SpeedControl();

        // Cancel gravity on slope
        _handler.RB.useGravity = !OnSlope();
    }

    private void SpeedControl() {
        if(OnSlope() && !_exitingSlope) {
            // Limiting speed on slopes
            if(_handler.RB.linearVelocity.magnitude > _totalSpeed)
                _handler.RB.linearVelocity = _handler.RB.linearVelocity.normalized * _totalSpeed;
        } else {
            // Limiting speed on floor / air
            Vector3 flatVel = new Vector3(_handler.RB.linearVelocity.x, 0f, _handler.RB.linearVelocity.z);
            if(flatVel.magnitude > _totalSpeed) {
                Vector3 limitedVel = flatVel.normalized * _totalSpeed;
                _handler.RB.linearVelocity = new Vector3(limitedVel.x, _handler.RB.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump() {
        _exitingSlope = true;
        _handler.RB.linearVelocity = new Vector3(_handler.RB.linearVelocity.x, 0f, _handler.RB.linearVelocity.z);
        _handler.RB.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
        // Claude added cartoonsh snap
        _handler.RB.AddForce(_moveDir.normalized * (_totalSpeed * 0.4f), ForceMode.Impulse);
    }

    private void ResetJump() {
        _readyToJump = true;
        _exitingSlope = false;
    }

    private bool OnSlope() {
        if(Physics.Raycast(transform.position, Vector3.down, out _slopeHit, 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, _slopeHit.normal);
            return angle < _maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection() {
        return Vector3.ProjectOnPlane(_moveDir, _slopeHit.normal).normalized;
    }

    // Stun stuff
    private bool FallingFast() {
        return _handler.RB.linearVelocity.y < -_maxVelocity;
    }

    // Setters used to ensure the stats are accurate to boosts
    private void SetMoveSpeed() { _totalSpeed = _moveSpeed + _boostSpeed; }
    private void SetSprintSpeed() { _totalSpeed = _moveSpeed * _sprintSpeedMult + _boostSpeed; }
    private void SetCrouchSpeed() { _totalSpeed = _moveSpeed * _crouchSpeedMult + _boostSpeed; }
    private void SetStaminaDrain() { _totalStaminaDrain = _staminaDrain + _boostStaminaDrain; }

    // Boosters
    public void BoostSpeed(float boost) { _boostSpeed += boost; }
    public void BoostStaminaDrain(float boost) { _boostStaminaDrain += boost; }
}
