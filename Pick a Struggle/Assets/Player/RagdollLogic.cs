using PurrNet;
using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

public class RagdollLogic : NetworkIdentity
{
    private Animator anim;

    [Header("Needed Transforms")]
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private Transform bodyBone;
    [SerializeField] private Transform player;
    [SerializeField] private Transform characterModel;

    [Header("Ragdoll")]
    public float weight;

    public bool ragdollActive = false;

    private Rigidbody[] rigidbodies;
    private CharacterJoint[] joints;
    private Collider[] colliders;

    private Transform[] transforms;
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;

    private Rigidbody rbBody;
    private Rigidbody rb;

    private Vector3 bodyPos;

    private void Awake() {
        anim = GetComponentInChildren<Animator>();
        rbBody = bodyBone.GetComponent<Rigidbody>();
        rb = GetComponent<Rigidbody>();

        characterModel = ragdollRoot.parent;
        player = characterModel.parent;

        rigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        joints = ragdollRoot.GetComponentsInChildren<CharacterJoint>();
        colliders = ragdollRoot.GetComponentsInChildren<Collider>();

        transforms = ragdollRoot.GetComponentsInChildren<Transform>();
        initialPositions = new Vector3[transforms.Length];
        initialRotations = new Quaternion[transforms.Length];

        // Set the initial rotations
        for(int i = 0; i < transforms.Length; i++) {
            initialPositions[i] = transforms[i].localPosition;
            initialRotations[i] = transforms[i].localRotation;
        }

        // Initial body position
        bodyPos = bodyBone.transform.localPosition;

        if(ragdollActive) {
            EnableRagdoll(Vector3.zero);
        } else {
            DisableRagdoll();
        }

        weight = 0;
        foreach(Rigidbody rb in rigidbodies)
            weight += rb.mass;
    }

    public void EnableRagdoll(Vector3 force) {
        ragdollActive = true;

        // Detatch the model
        characterModel.SetParent(null);

        anim.enabled = false;
        foreach(CharacterJoint joint in joints) 
            joint.enableCollision = true;
        
        foreach(Collider collider in colliders) 
            collider.enabled = true;

        foreach(Rigidbody rb in rigidbodies) {
            rb.linearVelocity = Vector3.zero;
            rb.isKinematic = false;
            rb.detectCollisions = true;
            rb.useGravity = true;
        }

        // Apply force direction
        TossRagdoll(force, 1);
    }

    private void DisableRagdoll() {
        foreach(CharacterJoint joint in joints)
            joint.enableCollision = false;

        foreach(Collider collider in colliders)
            collider.enabled = false;

        foreach(Rigidbody rb in rigidbodies) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.useGravity = false;
        }
    }

    public void EnableAnimator() {
        //StartCoroutine(GetUpRoutine());

        // Align the player container
        AlignPosition();

        // Kill lingering velocities
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset body parts
        DisableRagdoll();

        // Re-attatch model
        characterModel.SetParent(player);
        characterModel.localPosition = Vector3.zero;
        characterModel.localRotation = Quaternion.identity;

        ragdollActive = false;
        anim.enabled = true;

        // Fix animations
        anim.Rebind();
        anim.Update(0f);
    }

    private void AlignPosition() {
        Vector3 originalHipPosition = bodyBone.position;
        transform.position = originalHipPosition;

        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo))
            transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);

        bodyBone.position = originalHipPosition;
    }

    public void TossRagdoll(Vector3 dir, float mult){
        Vector3 force = dir * mult;

        if(ragdollActive)
            rbBody.AddForce(force * weight, ForceMode.Impulse);
    }
}
