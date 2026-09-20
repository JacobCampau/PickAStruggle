using PurrNet;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlayerInteraction : NetworkIdentity
{
    private PlayerCombat _playerCombat;
    private PlayerHotbarController _playerHotbarController;
    private PlayerHotbar _playerHotbar;

    [Header("Player Arm Settings")]
    [SerializeField] private float _distanceFromCameraToHead = 4.5f;
    [SerializeField] private float _playerReach;
    [SerializeField] private LayerMask _itemLayers;
    private Transform _playerCamera;
    [SerializeField] private Transform _rightHand;
    [SerializeField] private float _dropSpeed = 2f;

    private Transform _currentItemSelected;
    private Item _currItemComp;

    [Header("Keybinds")]
    public KeyCode pickup;
    public KeyCode drop;

    // Global reach raycast
    private RaycastHit _raycastHit;

    private void Awake() {
        _playerCombat = GetComponent<PlayerCombat>();
        _playerHotbarController = GetComponent<PlayerHotbarController>();
        _playerHotbar = GetComponentInChildren<PlayerHotbar>();
    }

    private void Update() {
        if(_playerCombat.activeCameraTransform == null || _playerCombat.activeCameraTransform != _playerCamera)
            _playerCamera = _playerCombat.activeCameraTransform;

        ArmReachAndPickUp();
        CheckForItemDrop();
    }

    private void ArmReachAndPickUp() {
        if(_playerCamera == null) return;

        Debug.DrawRay(_playerCamera.position, _playerCamera.forward * (_distanceFromCameraToHead + _playerReach), Color.purple);
        if(Physics.Raycast(_playerCamera.position, _playerCamera.forward, out _raycastHit, _distanceFromCameraToHead + _playerReach, _itemLayers)) {
            if(_raycastHit.transform.GetComponent<Item>() == null)
                Debug.Log("Missing outline component");

            // Selected item does have an outline script
            if(_currentItemSelected != null) {
                if(_currentItemSelected != _raycastHit.transform) {
                    _currItemComp.SetSelectedState(false);
                }
            }
            _currentItemSelected = _raycastHit.transform;

            if(_currentItemSelected.GetComponent<Item>() == null)
                Debug.Log("Missing item component");

            _currItemComp = _currentItemSelected.GetComponent<Item>();
            _currItemComp.SetSelectedState(true);

            if(Input.GetKeyDown(pickup)) {
                // Player has decided to pick up a selected item
                Item item = _currItemComp;
                item.SetSelectedState(false);
                _currentItemSelected = null;
                _playerHotbarController.RequestPickup(item);
            }

        } else {
            // Turn off the outline for the prior object
            if(_currentItemSelected != null && _currentItemSelected.GetComponent<Item>() != null) {
                _currentItemSelected.GetComponent<Item>().SetSelectedState(false);
                _currentItemSelected = null;
            }
        }
    }

    private void CheckForItemDrop() {
        if(Input.GetKeyDown(drop)) {
            _playerHotbarController.RequestDrop(_dropSpeed);
        }
    }
}
