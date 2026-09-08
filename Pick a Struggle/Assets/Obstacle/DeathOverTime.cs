using UnityEngine;

public class DeathOverTime : MonoBehaviour
{
    public float timer;
    private float _timerMax;

    private void Start() {
        _timerMax = timer;
        timer = 0f;
    }

    private void Update() {
        timer += Time.deltaTime;
        if(timer > _timerMax) {
            Destroy(gameObject);
        }
    }
}
