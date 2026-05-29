using PurrNet;
using UnityEngine;

public class PlayerHitbox : NetworkIdentity
{
    private PlayerCombat _combat;
    private PlayerAnimator _animator;

    void Awake(){
        _combat = GetComponent<PlayerCombat>();
        _animator = GetComponent<PlayerAnimator>();
    }

    // Trigger based detections -> player is moving or animated
    private void OnTriggerEnter(Collider other){

    }

    private void OnTriggerStay(Collider other){

    }

    private void OnTriggerExit(Collider other){

    }

    // Collision based detection -> Ragdoll is activated
    private void OnCollisionEnter(Collision other){
        if(other.gameObject.tag == "Environment"){
            _animator.FallParticles(transform);
        }
    }

    private void OnCollisionStay(Collision other){

    }

    private void OnCollisionExit(Collision other){

    }
}