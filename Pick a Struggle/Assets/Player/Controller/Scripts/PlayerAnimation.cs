using System.Linq;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float locomotionBlendSpeed = 0.02f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    private PlayerController _playerController;
    private PlayerActionsInput _playerActionsInput;

    // Locomotion
    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isIdlingHash = Animator.StringToHash("isIdling");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");

    // Camera
    private static int isRotatingToTargetHash = Animator.StringToHash("isRotatingToTarget");
    private static int rotationMismatchHash = Animator.StringToHash("rotationMismatch");

    // Actions
    public int[] actionHashes;
    private static int isPlayingActionHash = Animator.StringToHash("isPlayingAction");
    private static int isHumpingHash = Animator.StringToHash("isHumping");
    private static int isWavingHash = Animator.StringToHash("isWaving");

    // Ragdoll
    private static int isRagdollHash = Animator.StringToHash("isRagdoll");

    private Vector3 _currentBlendInput = Vector3.zero;

    private float _sprintMaxBlendValue = 1.5f;
    private float _runMaxBlendValue = 1f;
    private float _crouchMaxBlendValue = 0.5f;
    private void Awake() {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
        _playerController = GetComponent<PlayerController>();
        _playerActionsInput = GetComponent<PlayerActionsInput>();

        actionHashes = new int[] { isHumpingHash }; //interruptables ONLY
    }

    private void Update() {
        UpdateAnimationState();
    }

    private void UpdateAnimationState() {
        bool isIdling = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Idling;
        bool isRunning = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Running;
        bool isSprinting = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Sprinting;
        bool isJumping = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Jumping;
        bool isFalling = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Falling;
        bool isGrounded = _playerState.InGroundedState();
        bool isPlayingAction = actionHashes.Any(hash => _animator.GetBool(hash));
        bool isRagdoll = _playerState.CurrentPlayerMovementState == EPlayerMovementState.Ragdoll;

        bool isRunBlendValue = isRunning || isJumping || isFalling;
        Vector2 inputTarget = isSprinting ? _playerLocomotionInput.MovementInput * _sprintMaxBlendValue : 
                            isRunBlendValue ? _playerLocomotionInput.MovementInput * _runMaxBlendValue :
                                                _playerLocomotionInput.MovementInput * _crouchMaxBlendValue;
        
        _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

        _animator.SetBool(isGroundedHash, isGrounded);
        _animator.SetBool(isIdlingHash, isIdling);
        _animator.SetBool(isFallingHash, isFalling);
        _animator.SetBool(isJumpingHash, isJumping);
        _animator.SetBool(isRotatingToTargetHash, _playerController.IsRotatingToTarget);
        _animator.SetBool(isHumpingHash, _playerActionsInput.HumpPressed);
        _animator.SetBool(isPlayingActionHash, isPlayingAction);
        _animator.SetBool(isWavingHash, _playerActionsInput.WavePressed);
        _animator.SetBool(isRagdollHash, isRagdoll);

        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
        _animator.SetFloat(rotationMismatchHash, _playerController.RotationMismatch);
    }
}
