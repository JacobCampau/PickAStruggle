using PurrNet;
using UnityEngine;

public class PlayerHitbox : NetworkIdentity
{
    private PlayerCombat _combat;

    void Awake(){
        _combat = GetComponent<PlayerCombat>();
    }

    private void OnTriggerEnter(Collider other){

    }

    private void OnTriggerStay(Collider other){

    }

    private void OnTriggerExit(Collider other){

    }
}