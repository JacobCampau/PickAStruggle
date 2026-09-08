using UnityEngine;

public class HealthBarController : MonoBehaviour
{
    public Transform bar;
    private PlayerCombat _playerCombat;

    private void Start() {
        _playerCombat = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCombat>();
    }

    private void Update() {
        float xScale = _playerCombat.CurrentPlayerHealth/_playerCombat.TotalHealth;
        bar.localScale = new Vector3(xScale, 1f, 1f);
    }
}
