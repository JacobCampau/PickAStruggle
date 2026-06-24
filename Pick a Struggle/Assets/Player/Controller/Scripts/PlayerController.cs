using System;
using Unity.Cinemachine;
using UnityEngine;

[DefaultExecutionOrder(-1)]

public class PlayerController : MonoBehaviour
{
    #region Class Variables
    [Header("Components")]
    [SerializeField] private CharacterController _characterController;
    [SerializeField] private Camera _playerCamera;

    public float RotationMismatch { get; private set; } = 0f;
    public bool IsRotatingToTarget { get; private set; } = false;

    [Header("Debug")]
    public float timeScale = 1f;

    [Header("Base Movement")]
    public float crouchAcceleration = 0.15f;
    private float crouchSpeed;
    public float runAcceleration = 0.25f;
    private float runSpeed;
    public float sprintAcceleration = 0.5f;
    private float sprintSpeed;
    public float inAirAcceleration = 0.15f;
    public float drag = 20f;
    public float inAirDrag = 5f;
    public float movingThreshold = 0.01f;
    public float gravity = 25f;
    public float terminalVelocity = 50f;
    public float jumpSpeed = 1.0f;

    [Header("Animation")]
    public float playerModelRotationSpeed = 10f;
    public float rotateToTargetTime = 0.25f;

    [Header("Ragdoll")]
    public float fallDamageVelocity = 8f;

    [Header("Camera Settings")]
    public float lookSenseH = 0.1f;
    public float lookSenseV = 0.1f;
    public float lookLimitV = 89f;

    [Header("Environment Details")]
    [SerializeField] private LayerMask _groundLayers;
    public float groundCheckOffset = 0f;
    public float groundCheckRadius = 0.2f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerActionsInput _playerActionInput;

    private PlayerState _playerState;
    private PlayerStatHandler _statHandler;
    private PlayerCombat _playerCombat;

    private Vector2 _cameraRotation = Vector2.zero;
    private Vector2 _playerTargetRotation = Vector2.zero;
    private Vector3 _maxPlayerVelocity = Vector3.zero;

    private bool _jumpedLastFrame = false;
    private bool _isRotatingClockwise = false;

    private float _rotatingToTargetTimer = 0f;
    private float _verticalVelocity = 0f;
    private float _antiBump;
    private float _stepOffset;
    private float _airborneTime = 0f;

    private EPlayerMovementState _lastMovementState = EPlayerMovementState.Falling;
    #endregion

    #region Start Methods
    private void Awake() {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerActionInput = GetComponent<PlayerActionsInput>();

        _playerState = GetComponent<PlayerState>();
        _statHandler = GetComponent<PlayerStatHandler>();
        _playerCombat = GetComponent<PlayerCombat>();

        _stepOffset = _characterController.stepOffset;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start() {
        // Set the stats
        crouchSpeed = _statHandler.Stats.crouchSpeed;
        runSpeed = _statHandler.Stats.runSpeed;
        sprintSpeed = _statHandler.Stats.sprintSpeed;

        _antiBump = sprintSpeed;
    }
    #endregion

    #region Update Logic
    private void Update() {
        UpdateMovementState();
        VerticalMovement();
        LateralMovement();

        Time.timeScale = timeScale;
    } 

    void UpdateMovementState() {
        _lastMovementState = _playerState.CurrentPlayerMovementState;

        bool isMovementInput = _playerLocomotionInput.MovementInput != Vector2.zero;
        bool isMovingLaterally = IsMovingLaterally();
        bool isSprinting = _playerLocomotionInput.SprintToggledOn && isMovingLaterally;
        bool isCrouching = _playerLocomotionInput.CrouchToggledOn;
        bool isGrounded = IsGrounded();
        bool isRagdoll = _playerState.CurrentRagdollState != ERagdollState.Complete;

        Vector3 movingDirection = _characterController.velocity;

        EPlayerMovementState lateralState = isRagdoll ? EPlayerMovementState.Ragdoll :
                                            isCrouching ? EPlayerMovementState.Crouching :
                                            isSprinting ? EPlayerMovementState.Sprinting :
                                            isMovingLaterally || isMovementInput ? EPlayerMovementState.Running : EPlayerMovementState.Idling;

        _playerState.SetPlayerMovementState(lateralState);

        // Airborn
        if((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y > 0f && !isRagdoll) {
            _playerState.SetPlayerMovementState(EPlayerMovementState.Jumping);
            _jumpedLastFrame = false;
            _characterController.stepOffset = 0f;
        }else if((!isGrounded || _jumpedLastFrame) && _characterController.velocity.y <= 0f && !isRagdoll) {
            _playerState.SetPlayerMovementState(EPlayerMovementState.Falling);
            _jumpedLastFrame = false;
            _characterController.stepOffset = 0f;
        } else {
            _characterController.stepOffset = _stepOffset;
        }

        // Ragdoll
        if(_playerState.CurrentPlayerMovementState == EPlayerMovementState.Falling) {
            if(Mathf.Abs(movingDirection.y) > Mathf.Abs(_maxPlayerVelocity.y)) {
                _maxPlayerVelocity = movingDirection;
            }
        }

        if(_lastMovementState == EPlayerMovementState.Falling && isGrounded) {
            if(Mathf.Abs(_maxPlayerVelocity.y) > fallDamageVelocity) {
                // Set ragdoll and get the wanted velocity
                Vector3 fallDirection = new Vector3(_maxPlayerVelocity.x, 0f, _maxPlayerVelocity.z);
                _playerCombat.FallDamage(_maxPlayerVelocity, fallDirection.magnitude);
            }
            _maxPlayerVelocity = Vector3.zero;
        }
    }

    void VerticalMovement() {
        bool isGrounded = _playerState.InGroundedState();

        _verticalVelocity -= gravity * Time.deltaTime;

        if(isGrounded && _verticalVelocity < 0)
            _verticalVelocity = -_antiBump;

        if(_playerLocomotionInput.JumpPressed && isGrounded) {
            _verticalVelocity += Mathf.Sqrt(jumpSpeed * 3 * gravity);
            _jumpedLastFrame = true;
        }

        if(_playerState.IsStateGroundedState(_lastMovementState) && !isGrounded) {
            _verticalVelocity += _antiBump;
        }

        if(Mathf.Abs(_verticalVelocity) > Mathf.Abs(terminalVelocity))
            _verticalVelocity = -1f * Mathf.Abs(terminalVelocity);
    }

    void LateralMovement() {
        bool isSprinting = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Sprinting;
        bool isCrouching = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Crouching;
        bool isGrounded = _playerState.InGroundedState();
        bool canMove = _playerState.CurrentPlayerMovementState != EPlayerMovementState.Ragdoll;

        float lateralAcceleration = !isGrounded ? inAirAcceleration :
                                    isCrouching ? crouchAcceleration :
                                    isSprinting ? sprintAcceleration : runAcceleration;
        float clampLateralMagnitude = !isGrounded ? (sprintSpeed + runSpeed) / 2 :
                                    isCrouching ? crouchSpeed :
                                    isSprinting ? sprintSpeed : runSpeed;

        Vector3 cameraForwardXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 cameraRightXZ = new Vector3(_playerCamera.transform.right.x, 0f, _playerCamera.transform.right.z).normalized;
        Vector3 movementDirection = cameraRightXZ * _playerLocomotionInput.MovementInput.x + cameraForwardXZ * _playerLocomotionInput.MovementInput.y;

        Vector3 movementDelta = movementDirection * lateralAcceleration;
        Vector3 newVelocity = _characterController.velocity + movementDelta;

        // Airborne frames
        if(!isGrounded) _airborneTime += Time.deltaTime;
        else _airborneTime = 0f;
        bool wallCheckReady = _airborneTime > 0.1f;

        // Add drag
        float dragManitude = isGrounded ? drag : inAirDrag;
        Vector3 currentDrag = newVelocity.normalized * dragManitude * Time.deltaTime;
        newVelocity = (newVelocity.magnitude > dragManitude * Time.deltaTime) ? newVelocity - currentDrag : Vector3.zero;
        newVelocity = Vector3.ClampMagnitude(new Vector3(newVelocity.x, 0f, newVelocity.z), clampLateralMagnitude);
        newVelocity.y += _verticalVelocity;
        newVelocity = (!isGrounded && wallCheckReady) ? HandleSteepWalls(newVelocity) : newVelocity;

        // Check state if dead/ragdoll
        if(!canMove) return;

        // Check animations
        Vector3 lateralMovement = new Vector3(newVelocity.x, 0f, newVelocity.z);
        if(lateralMovement != Vector3.zero) {
            _playerActionInput.SetHumpPressedFalse();
        }

        // ONLY CALL ONCE PER FRAME!!
        _characterController.Move(newVelocity * Time.deltaTime);
    }

    Vector3 HandleSteepWalls(Vector3 velocity) {
        Vector3 normal = CharacterControlUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;

        if(!validAngle && _verticalVelocity < 0f) {
            velocity = Vector3.ProjectOnPlane(velocity, normal);
        } 

        return velocity;
    }
    #endregion

    #region Late Update Logic
    private void LateUpdate() {
        CameraRotation();
    }

    void CameraRotation() {
        _cameraRotation.x += lookSenseH * _playerLocomotionInput.LookInput.x;
        _cameraRotation.y = Mathf.Clamp(_cameraRotation.y - lookSenseV * _playerLocomotionInput.LookInput.y, -lookLimitV, lookLimitV);

        _playerTargetRotation.x += transform.eulerAngles.x + lookSenseH * _playerLocomotionInput.LookInput.x;

        float rotationTolerance = 90f;
        bool isIdling = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Idling;
        bool isRagoll = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Ragdoll;
        IsRotatingToTarget = _rotatingToTargetTimer > 0;

        if(!isRagoll) {
            if(!isIdling) {
                RotatePlaterToTarget();
            }
            else if(Mathf.Abs(RotationMismatch) > rotationTolerance || IsRotatingToTarget) {
                UpdateIdleRotation(rotationTolerance);
            }
        }

        _playerCamera.transform.rotation = Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0f);

        Vector3 camForwardProjectedXZ = new Vector3(_playerCamera.transform.forward.x, 0f, _playerCamera.transform.forward.z).normalized;
        Vector3 crossProduct = Vector3.Cross(transform.forward, camForwardProjectedXZ);
        float sign = Mathf.Sign(Vector3.Dot(crossProduct, transform.up));
        RotationMismatch = sign * Vector3.Angle(transform.forward, camForwardProjectedXZ);
    }

    void UpdateIdleRotation(float rotationTolerance) {
        if(Mathf.Abs(RotationMismatch) > rotationTolerance) {
            _rotatingToTargetTimer = rotateToTargetTime;
            _isRotatingClockwise = RotationMismatch > rotationTolerance;
        }
        _rotatingToTargetTimer -= Time.deltaTime;

        if(_isRotatingClockwise && RotationMismatch > 0 || !_isRotatingClockwise && RotationMismatch < 0) {
            _playerActionInput.SetHumpPressedFalse();
            RotatePlaterToTarget();
        }
    }

    void RotatePlaterToTarget() {
        Quaternion targetRotationX = Quaternion.Euler(0f, _playerTargetRotation.x, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotationX, playerModelRotationSpeed * Time.deltaTime);
    }
    #endregion

    #region State Checks
    bool IsMovingLaterally() {
        Vector3 lateralVelocity = new Vector3(_characterController.velocity.x, 0f, _characterController.velocity.z);

        return lateralVelocity.magnitude > movingThreshold;
    }

    bool IsGrounded() {
        bool grounded = _playerState.InGroundedState() ? IsGroundedWhileGrounded() : IsGroundedWhileAirborne();

        return grounded;
    }

    bool IsGroundedWhileGrounded() {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + groundCheckOffset, transform.position.z);
        bool grounded = Physics.CheckSphere(spherePosition, groundCheckRadius, _groundLayers, QueryTriggerInteraction.Ignore);
        return grounded;
    }

    bool IsGroundedWhileAirborne() {
        Vector3 normal = CharacterControlUtils.GetNormalWithSphereCast(_characterController, _groundLayers);
        float angle = Vector3.Angle(normal, Vector3.up);
        bool validAngle = angle <= _characterController.slopeLimit;
        return _characterController.isGrounded && validAngle;
    }

    private void OnDrawGizmosSelected() {
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y + groundCheckOffset, transform.position.z);

        // Change color to green if it detects something, otherwise red
        bool isColliding = Physics.CheckSphere(spherePosition, groundCheckRadius, _groundLayers, QueryTriggerInteraction.Ignore);
        Gizmos.color = isColliding ? Color.green : Color.red;

        // Draw a wireframe sphere so it doesn't block your view
        Gizmos.DrawWireSphere(spherePosition, groundCheckRadius);
    }
    #endregion
}
