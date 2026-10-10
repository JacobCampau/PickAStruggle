using PurrNet;
using System;
using UnityEngine;
using static UnityEngine.Audio.GeneratorInstance;

[RequireComponent(typeof(Rigidbody))]
public class Item : NetworkIdentity
{
    public ItemData data;
    
    private Rigidbody _rb;
    private Outline _outline;
    private Renderer[] _renderers;
    private Collider[] _colliders;
    private Behaviour[] _netSync;

    [SerializeField] private Vector3 _thrownRotation;
    public bool thrown = false;

    [field: SerializeField] public bool IsHeld { get; private set; } = false;

    [Header("Collision Timer")]
    [SerializeField] float _activatePlayerCollisionTimer = 0.2f;
    private Collider[] _playerColliders;

    private void Start() {
        _rb = GetComponent<Rigidbody>();
        _outline = GetComponent<Outline>();
        _renderers = GetComponentsInChildren<Renderer>(true);
        _colliders = GetComponentsInChildren<Collider>(true);

        _netSync = new Behaviour[] {
            GetComponent<NetworkTransform>(),
            GetComponent<NetworkRigidbody>()
        };

        if(_outline != null) _outline.enabled = false;
    }

    public void SetHeld(Transform hand) {
        IsHeld = true;

        SetNetSync(false);

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;

        SetColliders(false);

        transform.SetParent(hand, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if(_outline != null) _outline.enabled = false;
    }

    public void Release(Vector3 throwDir, float force) {
        IsHeld = false;

        transform.SetParent(null, true);
        SetVisible(true);
        SetColliders(true);

        _rb.isKinematic = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        SetNetSync(true);

        // Only the server applies the impulse; NetworkRigidbody syncs the result.
        if(isServer && force > 0f)
            _rb.AddForce(throwDir.normalized * force, ForceMode.Impulse);
    }

    public void Thrown(Vector3 throwDir, float force) {
        // Handle player collisions
        IgnorePlayerCollision();
        Invoke(nameof(ActivatePlayerCollision), _activatePlayerCollisionTimer);

        // Other thrown logic
        IsHeld = false;

        transform.SetParent(null, true);
        SetVisible(true);
        SetColliders(true);

        _rb.isKinematic = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        SetNetSync(true);

        // Only the server applies the impulse; NetworkRigidbody syncs the result.
        if(isServer && force > 0f) {
            _rb.AddForce(throwDir * force, ForceMode.Impulse);
            _rb.angularVelocity = transform.TransformDirection(_thrownRotation * Mathf.Deg2Rad);
            thrown = true;
        }
    }

    // Hide/show without deactivating the networked GameObject.
    public void SetVisible(bool visible) {
        foreach(var r in _renderers)
            if(r != null) r.enabled = visible;
    }

    public void SetSelectedState(bool val) {
        if(_outline != null) _outline.enabled = val;
    }

    private void SetColliders(bool enabled) {
        foreach(var c in _colliders)
            if(c != null) c.enabled = enabled;
    }

    private void SetNetSync(bool enabled) {
        foreach(var b in _netSync)
            if(b != null) b.enabled = enabled;
    }

    // Collider logic
    private void IgnorePlayerCollision() {
        // Get the player colliders
        Transform parent = transform.root;
        _playerColliders = parent.GetComponentsInChildren<Collider>();
        
        if(_playerColliders == null) return;

        SetIgnoreColliderCollision(true);
    }

    private void ActivatePlayerCollision() {
        if(_playerColliders == null) return;

        SetIgnoreColliderCollision(false);

        _playerColliders = null;
    }

    private void SetIgnoreColliderCollision(bool truth) {
        foreach(Collider playerCol in _playerColliders) {
            foreach(Collider itemCols in _colliders) {
                Physics.IgnoreCollision(itemCols, playerCol, truth);
            }
        }
    }
}