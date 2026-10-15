using UnityEngine;

public class PlayerBodyItem : MonoBehaviour
{
    private PlayerState _playerState;
    private Item _bodyItemComp;

    private void Start()
    {
        _playerState = GetComponentInParent<PlayerState>();
        _bodyItemComp = GetComponent<Item>();

        _bodyItemComp.enabled = false;
    }

    private void Update()
    {
        if (_bodyItemComp != _playerState.IsDead) _bodyItemComp.enabled = _playerState.IsDead;
    }
}
