using UnityEngine;

public class PlayerHandling : NetworkIdentity
{
    [SerializeField] private bool debug;

    [Header("Key Binds")]
    public KeyCode grabItem = KeyCode.E;
    public KeyCode dropItem = KeyCode.Q;

    [Header("Hand Settings")]
    [Tooltip("The transform the picked-up item will be parented to (e.g. the player's hand bone).")]
    [SerializeField] private Transform handTransform;

    [Header("Pickup Settings")]
    [Tooltip("Maximum distance at which the player can pick up an item.")]
    [SerializeField] private float pickupRange = 3f;

    [Tooltip("Layer mask used for the raycast so it only hits pickupable objects.")]
    [SerializeField] private LayerMask pickupMask;

    [Header("Throw Settings")]
    [Tooltip("Force applied to the item when thrown.")]
    [SerializeField] private float throwForce = 10f;

    [Tooltip("Small upward angle (degrees) added to the throw direction.")]
    [SerializeField] private float throwUpwardAngle = 5f;

    private GameObject heldItem;
    private Rigidbody  heldRb;

    // Camera reference – tries the main camera; override if needed.
    private Camera cam;

    private void Awake(){
        cam = Camera.main;

        if (handTransform == null)
            Debug.LogWarning("[PlayerHandling] handTransform is not assigned. " + "Items will be parented to this GameObject instead.");
    }

    private void Update(){
        if(ragdoll.ragdollActive) return;

        if (Input.GetKeyDown(grabItem))
        {
            if (heldItem == null)
                TryPickUp();
        }

        if (Input.GetKeyDown(dropItem) && heldItem != null)
            Throw();

        if(debug)
            OnDrawGizmosSelected();
    }

    private void TryPickUp(){
        Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

        // Cancel out of function if no acceptable object is observed
        if (!Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask)) return;
        if (!hit.collider.CompareTag("Pickupable")) return;

        PickUp(hit.collider.gameObject);
    }

    private void PickUp(GameObject item){
        heldItem = item;
        heldRb = item.GetComponent<Rigidbody>();

        // Disable physics while held
        if (heldRb != null)
        {
            heldRb.isKinematic = true;
            heldRb.linearVelocity = Vector3.zero;
            heldRb.angularVelocity = Vector3.zero;
        }

        Transform parent = handTransform;
        item.transform.SetParent(parent);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    public void Drop(){
        if (heldItem == null) return;

        heldItem.transform.SetParent(null);

        if (heldRb != null)
            heldRb.isKinematic = false;

        heldItem = null;
        heldRb   = null;
    }

    private void Throw(){
        if (heldItem == null) return;

        // Detach before applying force
        heldItem.transform.SetParent(null);

        if (heldRb != null)
        {
            heldRb.isKinematic = false;

            // Throw direction: camera forward + slight upward tilt
            Vector3 throwDir = cam.transform.forward;
            throwDir = Quaternion.AngleAxis(-throwUpwardAngle, cam.transform.right) * throwDir;

            heldRb.AddForce(throwDir.normalized * throwForce, ForceMode.Impulse);
        }

        heldItem = null;
        heldRb = null;
    }

    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(cam.transform.position, cam.transform.forward * pickupRange);
    }
}
