using PurrNet;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    private PlayerHandling handler;

    [Header("Slot Counts")]
    public int hotbarSize = 3;
    public int inventorySize = 4;

    [Header("Hand Reference")]
    [SerializeField] private Transform handTransform;

    private Item[] hotbar;
    private Item[] inventory;

    private int activeSlot = 0;

    private void Start(){
        handler = GetComponent<PlayerHandling>();

        hotbar    = new Item[hotbarSize];
        inventory = new Item[inventorySize];

        handTransform = handler.handTransform;
        if (handTransform == null)
            Debug.LogWarning("[PlayerInventory] handTransform is not assigned.", this);
    }

    private void Update(){
        HandleSlotInput();
    }

    private void HandleSlotInput()
    {
        // Button change
        for (int i = 0; i < hotbarSize; i++){
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)){
                SetActiveSlot(i);
                return;
            }
        }

        // Scroll wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
            SetActiveSlot((activeSlot - 1 + hotbarSize) % hotbarSize);
        else if (scroll < 0f)
            SetActiveSlot((activeSlot + 1) % hotbarSize);
    }
    
    public void SetActiveSlot(int index)
    {
        if (index == activeSlot || index < 0 || index >= hotbarSize) return;

        // Hide item in old slot
        hotbar[activeSlot]?.gameObject.SetActive(false);

        activeSlot = index;

        // Show item in new slot
        hotbar[activeSlot]?.gameObject.SetActive(true);
    }
    
    public bool TryAddItem(Item item)
    {
        Transform parent = handTransform;

        // Look at hotbar first
        for (int i = 0; i < hotbarSize; i++)
        {
            if (hotbar[i] != null) continue;

            hotbar[i] = item;
            item.StoreInHand(parent);

            // Only show it if it lands in the active slot
            item.gameObject.SetActive(i == activeSlot);
            return true;
        }

        for (int i = 0; i < inventorySize; i++)
        {
            if (inventory[i] != null) continue;

            inventory[i] = item;
            item.StoreInHand(parent); // still parented to hand for easy retrieval
            item.gameObject.SetActive(false); // inventory items are always hidden
            return true;
        }

        return false; // no room
    }
    
    public void ThrowActiveItem(float defaultForce, Vector3 throwDir)
    {
        Item item = hotbar[activeSlot];
        if (item == null) return;

        float force = (item.data != null && item.data.throwForceOverride > 0f)
            ? item.data.throwForceOverride
            : defaultForce;

        item.Eject(throwDir, force);
        hotbar[activeSlot] = null;
    }

    public void DropActiveItem()
    {
        Item item = hotbar[activeSlot];
        if (item == null) return;

        item.Eject(Vector3.zero, 0f);
        hotbar[activeSlot] = null;
    }

    public Item GetHotbarItem(int i) => (i >= 0 && i < hotbarSize) ? hotbar[i] : null;
    public Item GetInventoryItem(int i) => (i >= 0 && i < inventorySize) ? inventory[i] : null;

    public void DebugPrint()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("── Hotbar ──────────────────────");
        for (int i = 0; i < hotbarSize; i++)
        {
            string mark = i == activeSlot ? " ◄" : "";
            string name = hotbar[i]?.data?.itemName ?? "(empty)";
            sb.AppendLine($"  [{i}] {name}{mark}");
        }
        sb.AppendLine("── Inventory ───────────────────");
        for (int i = 0; i < inventorySize; i++)
        {
            string name = inventory[i]?.data?.itemName ?? "(empty)";
            sb.AppendLine($"  [{i}] {name}");
        }
        Debug.Log(sb.ToString());
    }
}
