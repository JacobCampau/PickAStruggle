using PurrNet;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "Scriptable Objects/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemName = "Item";
    public string description = "";
    public GameObject icon;
}
