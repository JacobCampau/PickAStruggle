using UnityEngine;

public class BallShooter : MonoBehaviour
{
    [Header("Shooter Information")]
    public float speed;
    public Transform shooterPosition;
    public float timer;
    private float _timerMax;

    [Header("Ball Info")]
    public GameObject projectilePrefab;

    [Header("Particles")]
    public ParticleSystem parts;
    public Transform partLocation;

    private void Start() {
        _timerMax = timer;
        timer = 0;
    }

    private void Update() {
        timer += Time.deltaTime;
        if(timer > _timerMax) {
            timer = 0;
            SpawnProjectile();
            Instantiate(parts, partLocation.position, partLocation.rotation);
        }
    }

    void SpawnProjectile() {
        Vector3 direction = transform.forward;
        GameObject instance = Instantiate(projectilePrefab, shooterPosition.position, shooterPosition.rotation);
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if(rb != null) {
            rb.linearVelocity = direction * speed;
        }
    }
}
