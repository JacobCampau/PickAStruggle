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
    private LayerMask _playerBodyItemLayer;

    private void Start() {
        _playerCombat = GetComponentInParent<PlayerCombat>();
        _playerState = GetComponentInParent<PlayerState>();
        _playerController = GetComponentInParent<PlayerController>();

        // Hard coded so they never change
        _ragdollLayer = LayerMask.NameToLayer("Ragdoll");
        _inactiveRagdollLayer = LayerMask.NameToLayer("Ragdoll Hitbox");
        _playerBodyItemLayer = LayerMask.NameToLayer("Dead Player Part");
    }

    private void Update() {
        RagdollHitboxLayerControl();
    }

    void RagdollHitboxLayerControl() {
        if(_playerState.CurrentRagdollState != ERagdollState.Complete) {
            if (_playerState.IsDead)
            {
                gameObject.layer = _playerBodyItemLayer;
            }
            else
            {
                gameObject.layer = _ragdollLayer;
            }
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
            float hitForce = hitDir.magnitude * collision.rigidbody.mass;
            //Debug.Log($"Breakdown: speed was {hitDir.magnitude} and mass is {collision.rigidbody.mass}");
            if(!(hitForce > _playerController.thrownObjectVelocity)) return;

            _playerCombat.ThrownObjectDamage(hitDir.normalized, hitForce, gameObject);

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
