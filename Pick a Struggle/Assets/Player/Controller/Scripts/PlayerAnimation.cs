using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private float locomotionBlendSpeed = 0.02f;

    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;

    private static int inputXHash = Animator.StringToHash("inputX");
    private static int inputYHash = Animator.StringToHash("inputY");
    private static int inputMagnitudeHash = Animator.StringToHash("inputMagnitude");
    private static int isGroundedHash = Animator.StringToHash("isGrounded");
    private static int isFallingHash = Animator.StringToHash("isFalling");
    private static int isJumpingHash = Animator.StringToHash("isJumping");

    private Vector3 _currentBlendInput = Vector3.zero;

    private void Awake() {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
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

        Vector2 inputTarget = isSprinting ? _playerLocomotionInput.MovementInput * 1.5f : _playerLocomotionInput.MovementInput;
        _currentBlendInput = Vector3.Lerp(_currentBlendInput, inputTarget, locomotionBlendSpeed * Time.deltaTime);

        _animator.SetBool(isGroundedHash, isGrounded);
        _animator.SetBool(isFallingHash, isFalling);
        _animator.SetBool(isJumpingHash, isJumping);

        _animator.SetFloat(inputXHash, _currentBlendInput.x);
        _animator.SetFloat(inputYHash, _currentBlendInput.y);
        _animator.SetFloat(inputMagnitudeHash, _currentBlendInput.magnitude);
    }
}
