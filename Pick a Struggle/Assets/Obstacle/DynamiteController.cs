using PurrNet;
using System;
using UnityEngine;

public class DynamiteController : NetworkIdentity
{
    private Rigidbody _rb;
    private ExplodeOverTime _explodeController;

    [Header("Primed?")]
    public bool readyToExplode = false;
    public float hitTooFastVelocity = 5f;

    [Header("When Thrown")]
    public Vector3 throwSpin;

    private void Start() {
        _rb = GetComponent<Rigidbody>();
        _explodeController = GetComponent<ExplodeOverTime>();
        ExplosionEnabler(readyToExplode);
    }

    private void Update() {
        if(!readyToExplode) {
            if(_explodeController.enabled) {
                ExplosionEnabler(readyToExplode);
            }
        } else {
            if(!_explodeController.enabled) {
                ExplosionEnabler(readyToExplode);
            } 
        }
    }

    void ExplosionEnabler(bool truth) {
        _explodeController.enabled = truth;
        _explodeController.sparks.gameObject.SetActive(truth);
    }

    public void OnThrow() {
        readyToExplode = true;
        _rb.angularVelocity = throwSpin;
    }

    private void OnCollisionEnter(Collision collision) {
        if(!isServer) return;
        if(Mathf.Abs(collision.relativeVelocity.magnitude) > hitTooFastVelocity) {
            _explodeController.enabled = true;
            _explodeController.Explode();
        }
    }
}
