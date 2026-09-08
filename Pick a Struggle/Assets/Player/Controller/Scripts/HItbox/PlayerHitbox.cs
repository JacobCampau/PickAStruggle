using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    private PlayerCombat _playerCombat;
    private PlayerState _playerState;

    private LayerMask _ragdollLayer;
    private LayerMask _inactiveRagdollLayer;

    private void Start() {
        _playerCombat = GetComponentInParent<PlayerCombat>();
        _playerState = GetComponentInParent<PlayerState>();

        // Hard coded so they never change
        _ragdollLayer = LayerMask.NameToLayer("Ragdoll");
        _inactiveRagdollLayer = LayerMask.NameToLayer("Ragdoll Hitbox");
    }

    private void Update() {
        RagdollHitboxLayerControl();
    }

    void RagdollHitboxLayerControl() {
        if(_playerState.CurrentRagdollState != ERagdollState.Complete) {
            gameObject.layer = _ragdollLayer;
        } else {
            gameObject.layer = _inactiveRagdollLayer;
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.tag == "Throwable Object" && _playerState.CurrentRagdollState == ERagdollState.Complete) {
            _playerCombat.ThrownObjectDamage(collision.rigidbody.linearVelocity, collision.rigidbody.mass, gameObject);
        }
    }
}
