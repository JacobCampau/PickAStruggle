using PurrNet;
using UnityEngine;

public class PlayerGrabbing : NetworkIdentity
{
    private PlayerHandler _handler;
    private PlayerInventory inv;
    private RagdollLogic ragdoll;

    [SerializeField] private bool debug;

    [Header("Key Binds")]
    public KeyCode grabItem = KeyCode.E;
    public KeyCode dropItem = KeyCode.Q;

    [Header("Hand Settings")]
    public Transform handTransform;

    [Header("Pickup Settings")]
    [SerializeField] private float pickupRange = 3f;
    [SerializeField] private LayerMask pickupMask;

    [Header("Throw Settings")]
    [SerializeField] private float throwForce = 10f;
    [SerializeField] private float throwUpwardAngle = 5f;

    private Camera cam;

    private void Start(){
        cam = Camera.main;

        _handler = GetComponent<PlayerHandler>();
        ragdoll = GetComponent<RagdollLogic>();
        inv = GetComponent<PlayerInventory>();
    }

    private void Update(){
        if(_handler.playerState == PlayerHandler.EPlayerState.ragdoll) return;

        if (Input.GetKeyDown(grabItem)) 
            TryPickUp();

        if (Input.GetKeyDown(dropItem))
            TryThrow();

        if(debug)
            OnDrawGizmosSelected();
    }

    private void TryPickUp(){
        Ray ray = cam.ScreenPointToRay(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f));

        // Cancel out of function if no acceptable object is observed
        if (!Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupMask)) 
            return;

        if (!hit.collider.CompareTag("Pickupable")) 
            return;

        Item item = hit.collider.GetComponent<Item>();
        if (item == null){
            Debug.Log($"[PlayerHandling] '{hit.collider.name}' is tagged " + "'Pickupable' but has no Item component.", hit.collider);
            return;
        }

        if (!inv.TryAddItem(item))
            Debug.Log("[PlayerHandling] Cannot pick up — inventory is full.");
    }

    private void TryThrow(){
        Vector3 throwDir = Quaternion.AngleAxis(-throwUpwardAngle, cam.transform.right) * cam.transform.forward;
        inv.ThrowActiveItem(throwForce, throwDir);
    }

    private void OnDrawGizmosSelected(){
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(cam.transform.position, cam.transform.forward * pickupRange);
    }
}
