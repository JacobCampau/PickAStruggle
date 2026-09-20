using UnityEngine;

public class PlayerInterfaceState : MonoBehaviour
{
    [field: SerializeField] public EInterfaceState CurrentInterfaceState { get; private set; } = EInterfaceState.Playing;

    public void SetInterfaceState(EInterfaceState newState) {
        CurrentInterfaceState = newState;
    }
}

public enum EInterfaceState { 
    Playing = 0,
    Dead = 1,
    Paused = 2,
    Inventory = 3,
}