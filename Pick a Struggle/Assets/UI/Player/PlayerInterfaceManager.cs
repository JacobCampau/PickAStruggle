using UnityEngine;

public class PlayerInterfaceManager : MonoBehaviour
{
    private PlayerInterfaceState _playerInterfaceState;
    private PlayerState _playerState;
    private EInterfaceState _currentInterfaceState;
    private EInterfaceState _previousInterfaceState;

    [Header("Interfaces")] // ensure the editor view matches the order from interface state enum
    public GameObject[] interfaces;

    private void Awake() {
        _playerInterfaceState = GetComponent<PlayerInterfaceState>();
        _playerState = GetComponent<PlayerState>();

        UpdateActiveInterface((int)_playerInterfaceState.CurrentInterfaceState);
    }



    private void Update() {
        // Setting a link to the current state
        _currentInterfaceState = _playerInterfaceState.CurrentInterfaceState;

        // Interface logic
        if(_playerState.IsDead && _currentInterfaceState != EInterfaceState.Dead)
            _playerInterfaceState.SetInterfaceState(EInterfaceState.Dead);

        if(_currentInterfaceState != _previousInterfaceState)
            UpdateActiveInterface((int)_currentInterfaceState);

        // Update the previous state for the next frame
        _previousInterfaceState = _currentInterfaceState;
    }

    void UpdateActiveInterface(int index) {
        for(int i = 0; i < interfaces.Length; i++) {
            if(i == index)
                interfaces[i].SetActive(true);
            else
                interfaces[i].SetActive(false);
        }
    }
}