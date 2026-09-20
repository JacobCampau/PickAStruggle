using PurrNet;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : NetworkIdentity
{
    public ItemData data;
    
    private Rigidbody _rb;
    private Outline _outline;
    private Renderer[] _renderers;
    private Collider[] _colliders;
    private Behaviour[] _netSync;

    [field: SerializeField] public bool IsHeld { get; private set; } = false;

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

    // --- Old Code --- //
    /*
    [ObserversRpc]
    public void StoreInHand(Transform parent) {
        IsHeld = true;
        _rb.isKinematic = true;

        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        Debug.Log($"Pos: {transform.localPosition}, Rot: {transform.localRotation.eulerAngles}");

        gameObject.SetActive(false); // hidden until it is the active slot
    }

    [ObserversRpc]
    public void Eject(Vector3 throwDir, float force = 0f)
    {
        transform.SetParent(null);
        gameObject.SetActive(true);

        _rb.isKinematic = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        if (force > 0f)
            _rb.AddForce(throwDir.normalized * force, ForceMode.Impulse);
    }

    public void SetSelectedState(bool val) {
        _outline.enabled = val;
    }
    */
}
