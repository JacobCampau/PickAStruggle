using UnityEngine;

public class RagdollLogic : MonoBehaviour
{
    private Animator anim;

    [Header("Needed Transforms")]
    [SerializeField] private Transform ragdollRoot;
    [SerializeField] private Transform centerBone;

    [SerializeField] private Transform player;
    [SerializeField] private Transform baseCharacter;

    [Header("Ragdoll Cam Speed")]
    [SerializeField] private float followSpeed = 10f;

    public bool ragdollActive = false;

    private Rigidbody[] rigidbodies;
    private CharacterJoint[] joints;
    private Collider[] colliders;

    private void Awake() {
        anim = GetComponent<Animator>();

        baseCharacter = ragdollRoot.parent;
        player = baseCharacter.parent;

        rigidbodies = ragdollRoot.GetComponentsInChildren<Rigidbody>();
        joints = ragdollRoot.GetComponentsInChildren<CharacterJoint>();
        colliders = ragdollRoot.GetComponentsInChildren<Collider>();

        if(ragdollActive) {
            EnableRagdoll();
        } else {
            EnableAnimator();
        }
    }

    private void Update() {
        if(ragdollActive)
            player.position = Vector3.Lerp(player.position, centerBone.position, Time.deltaTime * followSpeed);
    }

    public void EnableRagdoll() {
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
    }

    public void EnableAnimator() {
        ragdollActive = false;

        // Re-attatch the model
        baseCharacter.SetParent(player);
        baseCharacter.localPosition = Vector3.zero;
        baseCharacter.localRotation = Quaternion.identity;

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
