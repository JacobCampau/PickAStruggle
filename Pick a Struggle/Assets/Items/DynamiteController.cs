using PurrNet;
using System;
using UnityEngine;

public class DynamiteController : NetworkIdentity
{
    private ExplodeOverTime _explodeController;
    private Item _item;

    [Header("Primed?")]
    public bool readyToExplode = false;
    public float hitTooFastVelocity = 5f;

    private void Start() {
        _explodeController = GetComponent<ExplodeOverTime>();
        _item = GetComponent<Item>();
        ExplosionEnabler(readyToExplode);
    }

    private void Update() {
        if(!isServer) return;

        if(!readyToExplode) {
            if(_explodeController.enabled) {
                ExplosionEnabler(readyToExplode);
            }
        } else {
            if(!_explodeController.enabled) {
                ExplosionEnabler(readyToExplode);
            } 
        }

        if(_item.thrown && !readyToExplode)
            readyToExplode = true;
    }

    void ExplosionEnabler(bool truth) {
        _explodeController.enabled = truth;
        _explodeController.sparks.gameObject.SetActive(truth);
    }

    private void OnCollisionEnter(Collision collision) {
        //if(!isServer) return;
        //if(Mathf.Abs(collision.relativeVelocity.magnitude) > hitTooFastVelocity) {
        //    _explodeController.enabled = true;
        //    _explodeController.Explode();
        //}
    }
}
