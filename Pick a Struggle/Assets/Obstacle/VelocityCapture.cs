using PurrNet;
using UnityEngine;
using UnityEngine.Assemblies;

public class VelocityCapture : NetworkIdentity
{
    private Rigidbody rb;
    private Vector3 _currentVelocity;
    public SyncVar<Vector3> PreviousVelocity;

    public bool isServerControlled = false;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        if(rb == null || (!isServer && isServerControlled)) return;
        _currentVelocity = rb.linearVelocity;
        PreviousVelocity.value = _currentVelocity;
    }

    private void FixedUpdate() {
        if(rb == null || (!isServer && isServerControlled)) return;
        if(PreviousVelocity != _currentVelocity) PreviousVelocity.value = _currentVelocity;
        if(_currentVelocity != rb.linearVelocity) _currentVelocity = rb.linearVelocity;
    }
}