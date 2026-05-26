using PurrNet;
using UnityEngine;

public class PlayerHitbox : NetworkIdentity
{
    private PlayerCombat _combat;

    void Awake(){
        _combat = GetComponent<PlayerCombat>();
    }

    // Trigger based detections -> player is moving or animated
    private void OnTriggerEnter(Collider other){

    }

    private void OnTriggerStay(Collider other){

    }

    private void OnTriggerExit(Collider other){

    }

    // Collision based detection -> Ragdoll is activated
    private void OnCollisionEnter(Collider other){
        if(other.gameObject.tag == "Environment"){
            _animator.FallParticles(transform);
        }
    }

    private void OnCollisionStay(Collider other){

    }

    private void OnCollisionExit(Collider other){

    }
}