using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Display name shown in the UI and logs.")]
    public string itemName = "Item";

    [TextArea(2, 4)]
    [Tooltip("Flavour text / description shown when the item is inspected.")]
    public string description = "";

    [Tooltip("Icon displayed in the hotbar and inventory slots.")]
    public Sprite icon;

    [Header("Throw Override")]
    [Tooltip("Per-item throw force. Leave at 0 to use PlayerHandling's default.")]
    public float throwForceOverride = 0f;

    [Header("Custom Data (extend as needed)")]
    [Tooltip("Generic float value — e.g. damage, healing, fuel amount.")]
    public float value = 0f;

    [Tooltip("Generic boolean flag — e.g. isStackable, isConsumable.")]
    public bool flag = false;
}
