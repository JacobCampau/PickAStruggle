using UnityEngine;

public class PlayerHeadTracking : MonoBehaviour
{
    private PlayerState _playerState;

    [Header("Transforms")]
    public Transform headTracker;
    public Transform cameraContainer;

    [Header("Track Settings")]
    public float defaultLookDistance = 4f;
    public float headYOffset = 0f;

    private void Awake() {
        _playerState = GetComponent<PlayerState>();
    }

    private void LateUpdate() {
        TrackingHandler();
    }

    void TrackingHandler() {
        if(_playerState.CurrentPlayerTrackingState == EHeadTrackingState.Default) {
            LeadMovement();
        }
    }

    void LeadMovement() {
        // Follow the camera rotation
        Vector3 camEuler = cameraContainer.eulerAngles;
        Vector3 lookDir = Quaternion.Euler(camEuler.x, camEuler.y, 0f) * Vector3.forward;

        headTracker.position = transform.position
                            + Vector3.up * headYOffset
                            + lookDir * defaultLookDistance;
    }
}

public enum EHeadTrackingState {
    Default = 1,
    Tracking = 2,
    Transitioning = 3,
}