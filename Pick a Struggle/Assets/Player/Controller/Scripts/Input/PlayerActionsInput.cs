using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionsInput : MonoBehaviour, PlayerControls.IPlayerActionMapActions {
    #region Class Variables
    private PlayerLocomotionInput _playerLocomotionInput;
    private PlayerState _playerState;
    public bool HumpPressed { get; private set; } = false;
    public bool WavePressed { get; private set; } = false;
    #endregion

    #region Startup
    private void Awake() {
        _playerLocomotionInput = GetComponent<PlayerLocomotionInput>();
        _playerState = GetComponent<PlayerState>();
    }

    private void OnEnable() {
        if(PlayerInputManager.Instance?.PlayerControls == null) {
            Debug.LogError("Player controls is not intitialized");
            return;
        }

        PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Enable();
        PlayerInputManager.Instance.PlayerControls.PlayerActionMap.SetCallbacks(this);
    }

    private void OnDisable() {
        if(PlayerInputManager.Instance?.PlayerControls == null) {
            Debug.LogError("Player controls is not intitialized");
            return;
        }

        PlayerInputManager.Instance.PlayerControls.PlayerActionMap.Disable();
        PlayerInputManager.Instance.PlayerControls.PlayerActionMap.RemoveCallbacks(this);
    }
    #endregion

    #region Updates
    private void Update() {
        if(_playerLocomotionInput.MovementInput != Vector2.zero ||
            _playerState.CurrentPlayerMovementState == EPlayerMovementState.Jumping ||
            _playerState.CurrentPlayerMovementState == EPlayerMovementState.Falling) {
            HumpPressed = false;
        }
    }

    public void SetHumpPressedFalse() {
        HumpPressed = false;
    }

    public void SetWavePressedFalse() {
        WavePressed = false;
    }
    #endregion

    #region Input Callbacks
    public void OnHumpDance(InputAction.CallbackContext context) {
        if(!context.performed) return;

        HumpPressed = true;
    }

    public void OnWave(InputAction.CallbackContext context) {
        if(!context.performed) return;

        WavePressed = true;
    }
    #endregion
}
