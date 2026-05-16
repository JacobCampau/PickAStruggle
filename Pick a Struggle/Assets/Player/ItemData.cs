using PurrNet;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemName = "Item";
    public string description = "";
    public Sprite icon;

    [Header("Throw Override")]
    public float throwForceOverride = 0f;

    [Header("Custom Data (extend as needed)")]
    public float value = 0f;
    public bool flag = false;
}
