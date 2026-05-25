using PurrNet;
using UnityEngine;

public class RagdollLogic : NetworkIdentity
{
    private PlayerHandler _handler;
    private PlayerAnimator _animator;
    private Animator _anim;

    [SerializeField] private bool _debug;

    [Header("Needed Transforms")]
    [SerializeField] private Transform _ragdollRoot;
    [SerializeField] private Transform _bodyBone;
    [SerializeField] private Transform _characterModel;
    [SerializeField] private Transform _orientation;

    [Header("Ragdoll")]
    public float weight;
    private bool _ragdollIsActive;

    private Rigidbody[] _rigidbodies;
    private CharacterJoint[] _joints;
    private Collider[] _colliders;

    private Rigidbody _rbBody;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;
    }

    private void Awake()
    {
        _handler = GetComponent<PlayerHandler>();
        _animator = GetComponent<PlayerAnimator>();
        _anim = GetComponentInChildren<Animator>();
        _rbBody = _bodyBone.GetComponent<Rigidbody>();

        _characterModel = _ragdollRoot.parent;

        _rigidbodies = _ragdollRoot.GetComponentsInChildren<Rigidbody>();
        _joints = _ragdollRoot.GetComponentsInChildren<CharacterJoint>();
        _colliders = _ragdollRoot.GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        // Starting state
        if (_handler.playerState == PlayerHandler.EPlayerState.ragdoll) {
            EnableRagdoll(Vector3.zero);
        } else {
            DisableRagdoll();
        }

        weight = 0;
        foreach(Rigidbody rb in _rigidbodies)
            weight += rb.mass;
    }

    public void EnableRagdoll(Vector3 force) {
        _ragdollIsActive = true;
        _handler.playerState = PlayerHandler.EPlayerState.ragdoll;

        // Detatch the model
        _characterModel.SetParent(null);

        _anim.enabled = false;
        foreach(CharacterJoint joint in _joints) 
            joint.enableCollision = true;
        
        foreach(Collider collider in _colliders) 
            collider.enabled = true;

        foreach(Rigidbody rb in _rigidbodies) {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.detectCollisions = true;
            rb.useGravity = true;
        }

        // Apply force direction
        TossRagdoll(force, 1);
    }

    private void DisableRagdoll() {
        foreach(CharacterJoint joint in _joints)
            joint.enableCollision = false;

        foreach(Collider collider in _colliders)
            collider.enabled = false;

        foreach(Rigidbody rb in _rigidbodies) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.useGravity = false;
        }
    }

    public void EnableAnimator() {

        // Reset body parts
        DisableRagdoll();

        // Kill lingering velocities
        _handler.RB.linearVelocity = Vector3.zero;
        _handler.RB.angularVelocity = Vector3.zero;

        // Re-attatch model
        _characterModel.SetParent(this.transform);

        // Exit truths
        _ragdollIsActive = false;
    }

    public void TossRagdoll(Vector3 dir, float mult){
        Vector3 force = dir * mult;

        if(_ragdollIsActive)
            _rbBody.AddForce(force * weight, ForceMode.Impulse);
    }
}