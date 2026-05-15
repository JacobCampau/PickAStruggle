using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class RagdollLogic : NetworkIdentity
{
    private Animator anim;

    [Header("Needed Transforms")]
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private Transform centerBone;

    [SerializeField] private Transform player;
    [SerializeField] private Transform baseCharacter;

    [Header("Ragdoll")]
    [SerializeField] private float followSpeed = 10;
    [SerializeField] private float getUpDuration = 0.4f;

    public float weight;

    public bool ragdollActive = false;
    private bool isGettingUp = false;

    private Rigidbody[] rigidbodies;
    private CharacterJoint[] joints;
    private Collider[] colliders;

    private Transform[] transforms;
    private Vector3[] initialPositions;
    private Quaternion[] initialRotations;

    Rigidbody rbBody;

    private void Awake() {
        anim = GetComponent<Animator>();
        rbBody = centerBone.GetComponent<Rigidbody>();

        baseCharacter = ragdollRoot.parent;
        player = baseCharacter.parent;

        rigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        joints = ragdollRoot.GetComponentsInChildren<CharacterJoint>();
        colliders = ragdollRoot.GetComponentsInChildren<Collider>();

        transforms = ragdollRoot.GetComponentsInChildren<Transform>();
        initialPositions = new Vector3[transforms.Length];
        initialRotations = new Quaternion[transforms.Length];

        // Set the initial positions and rotations
        for(int i = 0; i < transforms.Length; i++) {
            initialPositions[i] = transforms[i].localPosition;
            initialRotations[i] = transforms[i].localRotation;
        }

        if(ragdollActive) {
            EnableRagdoll(Vector3.zero);
        } else {
            EnableAnimator();
        }

        weight = 0;
        foreach(Rigidbody rb in rigidbodies)
            weight += rb.mass;
    }

    private void Update() {
        if(ragdollActive && !isGettingUp)
            player.position = Vector3.Lerp(player.position, centerBone.position, Time.deltaTime * followSpeed);
    }

    public void EnableRagdoll(Vector3 force) {
        ragdollActive = true;

        // Detatch the model
        baseCharacter.SetParent(null);

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
        rbBody.AddForce(force * weight, ForceMode.Impulse);
    }

    public void EnableAnimator() {
        StartCoroutine(GetUpRoutine());
    }

    private IEnumerator GetUpRoutine() {
        isGettingUp = true;

        // Re-attatch model
        baseCharacter.SetParent(player);
        Vector3 bcStartPos = baseCharacter.localPosition;

        // Get the current info
        Vector3[] startPositions = new Vector3[transforms.Length];
        Quaternion[] startRotations = new Quaternion[transforms.Length];
        for(int i = 0; i < transforms.Length; i++) {
            startPositions[i] = transforms[i].localPosition;
            startRotations[i] = transforms[i].localRotation;
        }

        // lerp transitions
        float elapsed = 0;
        while(elapsed < getUpDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / getUpDuration);

            baseCharacter.localPosition = Vector3.Lerp(bcStartPos, Vector3.zero, t);

            for(int i = 0; i < transforms.Length; i++) {
                transforms[i].localPosition = Vector3.Lerp(startPositions[i], initialPositions[i], t);
                transforms[i].localRotation = Quaternion.Lerp(startRotations[i], initialRotations[i], t);
            }
            yield return null;
        }

        // Snap to exact pose
        baseCharacter.localPosition = Vector3.zero;
        for(int i = 0; i < transforms.Length; i++) {
            transforms[i].localPosition = initialPositions[i];
            transforms[i].localRotation = initialRotations[i];
        }

        ragdollActive = false;
        isGettingUp = false;
        anim.enabled = true;
        foreach(CharacterJoint joint in joints)
            joint.enableCollision = false;

        foreach(Collider collider in colliders)
            collider.enabled = false;

        foreach(Rigidbody rb in rigidbodies) {
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.useGravity = false;
        }
    }
}
