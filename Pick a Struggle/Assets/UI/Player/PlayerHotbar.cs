using Unity.Cinemachine;
using UnityEngine;

public class PlayerHotbar : MonoBehaviour
{
    private PlayerHotbarController _playerHotbarController;

    [Header("Hotbar Graphics")]
    public Transform hotbarStartLocation;
    public GameObject hotbarGraphic;
    public float hotbarSlotDistance;

    [Space]
    public GameObject hotbarOutlinePrefab;
    public GameObject[] hotbarSlots;
    private GameObject _hotbarSelection;

    private int previousSlotPosition;

    private void Awake() {
        _playerHotbarController = GetComponentInParent<PlayerHotbarController>();
        _playerHotbarController.OnHotbarChanged += UpdateHotbar;

        _hotbarSelection = Instantiate(hotbarOutlinePrefab, hotbarStartLocation, false);
        hotbarSlots = new GameObject[_playerHotbarController.numberOfSlots];
        SetUpHotBar();

        // Initialize
        previousSlotPosition = 0;
    }

    private void Update() {
        if(_playerHotbarController.currentSlotSelection != previousSlotPosition)
            MoveSlotGraphic(_hotbarSelection, _playerHotbarController.currentSlotSelection);

        previousSlotPosition = _playerHotbarController.currentSlotSelection;
    }

    private void SetUpHotBar() {
        for(int i = 0; i < _playerHotbarController.numberOfSlots; i++) {
            hotbarSlots[i] = Instantiate(hotbarGraphic, hotbarStartLocation, false);
            MoveSlotGraphic(hotbarSlots[i], i);
        }
    }

    private void MoveSlotGraphic(GameObject element, int slotPos) {
        element.transform.localPosition = Vector3.right * hotbarSlotDistance * slotPos;
    }

    public void UpdateHotbar() {
        for(int i = 0; i < _playerHotbarController.numberOfSlots; i++) {
            Transform slot = hotbarSlots[i].transform;

            for(int c = slot.childCount - 1; c >= 0; c--)
                Destroy(slot.GetChild(c).gameObject);

            Item item = _playerHotbarController.HotbarItems[i];

            if(item != null)
                Instantiate(item.data.icon, slot, false);
        }
    }

    private void OnDestroy() { 
        if(_playerHotbarController != null) 
            _playerHotbarController.OnHotbarChanged -= UpdateHotbar; 
    }
}
