using PurrNet;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerHitbox : NetworkIdentity
{
    private PlayerCombat _playerCombat;
    private PlayerState _playerState;
    private PlayerController _playerController;

    private LayerMask _ragdollLayer;
    private LayerMask _inactiveRagdollLayer;

    private void Start() {
        _playerCombat = GetComponentInParent<PlayerCombat>();
        _playerState = GetComponentInParent<PlayerState>();
        _playerController = GetComponentInParent<PlayerController>();

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
        if(_playerState.IsDead) return;
        if(!_playerState.IsActiveHitbox) return;

        if(collision.gameObject.layer == LayerMask.NameToLayer("Throwable Objects") && collision.gameObject.GetComponent<VelocityCapture>() != null) {
            // Hit by a throwable object
            Vector3 hitDir = collision.gameObject.GetComponent<VelocityCapture>().PreviousVelocity;
            if(!(Mathf.Abs(hitDir.magnitude) > _playerController.thrownObjectVelocity)) return;

            _playerCombat.ThrownObjectDamage(hitDir.normalized, hitDir.magnitude * collision.rigidbody.mass, gameObject);

            if(_playerCombat._hitboxDebug) {
                Debug.Log($"Hitbox {gameObject.name} was hit by {collision.gameObject.name} at speed {hitDir.magnitude} with direction {hitDir}");
            }
        }
    }

    [ObserversRpc(runLocally: true)]
    public void ExplosionHit(Vector3 dir, float mult) {
        _playerCombat.ExplosionDamage(dir, mult, gameObject);
    }
}
