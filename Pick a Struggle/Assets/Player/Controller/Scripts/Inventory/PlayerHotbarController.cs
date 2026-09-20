using System;
using PurrNet;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHotbarController: NetworkIdentity {
    [Header("General Hotbar Settings")]
    public int numberOfSlots = 5;

    [Header("Hand of Player")]
    [SerializeField] private Transform _hand;

    [Header("Controls")]
    public int currentSlotSelection = 0;

    [field: SerializeField] public Item[] HotbarItems { get; private set; }

    public event Action OnHotbarChanged;

    private void Awake() {
        HotbarItems = new Item[numberOfSlots];
    }

    private void Update() {
        if(!isOwner) return;
        //CurrentSlotSelectionController();
        HandleScroll();
    }

    // --- Selection --- //
    private void HandleScroll() {
        if(Mouse.current == null) return;

        float scrollY = Mouse.current.scroll.ReadValue().y;
        if(scrollY == 0f) return;

        int next = currentSlotSelection;
        if(scrollY < 0f) next = (next + 1) % numberOfSlots;
        else next = (next - 1 + numberOfSlots) % numberOfSlots;

        currentSlotSelection = next;   // instant local feedback for the owner
        RefreshVisibility();
        OnHotbarChanged?.Invoke();
        SelectSlot(next);
    }

    [ServerRpc]
    private void SelectSlot(int slot) {
        ApplySelectSlot(slot);
    }

    [ObserversRpc(runLocally: true)]
    private void ApplySelectSlot(int slot) {
        currentSlotSelection = slot;
        RefreshVisibility();
        OnHotbarChanged?.Invoke();
    }

    // --- Pickup -- //
    // Called by the owning client (PlayerInteraction). Runs on the server.
    [ServerRpc]
    public void RequestPickup(Item item) {
        if(item == null || item.IsHeld) return;

        int slot = FindFreeSlot();
        if(slot < 0) return;

        ApplyPickup(item, slot);
    }

    [ObserversRpc(runLocally: true)]
    private void ApplyPickup(Item item, int slot) {
        if(item == null) return;

        HotbarItems[slot] = item;
        item.SetHeld(_hand);

        RefreshVisibility();
        OnHotbarChanged?.Invoke();
    }

    private int FindFreeSlot() {
        if(HotbarItems[currentSlotSelection] == null)
            return currentSlotSelection;

        for(int i = 0; i < numberOfSlots; i++)
            if(HotbarItems[i] == null) return i;

        return -1;
    }

    // --- Drop --- //
    [ServerRpc]
    public void RequestDrop(float dropSpeed) {
        if(HotbarItems[currentSlotSelection] == null) return;

        ApplyDrop(currentSlotSelection, transform.forward, dropSpeed);
    }

    [ObserversRpc(runLocally: true)]
    private void ApplyDrop(int slot, Vector3 dir, float force) {
        Item item = HotbarItems[slot];
        if(item == null) return;

        HotbarItems[slot] = null;
        item.Release(dir, force);

        RefreshVisibility();
        OnHotbarChanged?.Invoke();
    }

    // --- Visibility --- //
    private void RefreshVisibility() {
        for(int i = 0; i < numberOfSlots; i++) {
            if(HotbarItems[i] != null)
                HotbarItems[i].SetVisible(i == currentSlotSelection);
        }
    }

    // --- Old Code --- //
    /*
    public bool AddItemToHotbar(GameObject newItem) {
        if(_filledSlots >= numberOfSlots) return false;

        if(HotbarItems[currentSlotSelection] == null) {
            // Nothing in current slot, add it to the slot
            HotbarItems[currentSlotSelection] = newItem;
            _filledSlots++;

            return true;
        }

        bool placed = false;
        for(int i = 0; i < numberOfSlots; i++) {
            if(HotbarItems[i] == null) {
                HotbarItems[i] = newItem;
                placed = true;
            }
        }

        if(placed) _filledSlots++;

        return placed;
    }

    [ObserversRpc]
    public void SetCurrentItemActive(int slot) {
        for(int i = 0; i < numberOfSlots; i++) {
            if(HotbarItems[i] != null) {
                HotbarItems[i].SetActive(i == slot);
            }
        }
    }

    private void CurrentSlotSelectionController() {
        Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
        int tempScroll = currentSlotSelection;

        if(scrollDelta.y < 0) {
            tempScroll++;
            if(tempScroll >= numberOfSlots)
                tempScroll = 0;
        } else if(scrollDelta.y > 0) {
            tempScroll--;
            if(tempScroll < 0)
                tempScroll = numberOfSlots-1;
        }

        if(tempScroll != currentSlotSelection) {
            currentSlotSelection = tempScroll;
            SetCurrentItemActive(currentSlotSelection);
        }
    }

    public bool DropActiveItem(float dropSpeed) {
        if(HotbarItems[currentSlotSelection] != null) {
            HotbarItems[currentSlotSelection].GetComponent<Item>().Eject(transform.forward, dropSpeed);
            _filledSlots--;
            HotbarItems[currentSlotSelection] = null;
            return true;
        }

        return false;
    }
    */
}
