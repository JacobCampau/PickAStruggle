using UnityEngine;

public class SpeedCube: MonoBehaviour {
    [Header("Speeds")]
    public float timeBetweenPoints = 2f;

    [Header("Mass")]
    public float cubeMass = 10f;

    [Header("Between Transforms")]
    public Transform startPosition;
    public Transform endPosition;
    private float _elapsedTime = 0f;

    private void Update() {
        // Times
        _elapsedTime += Time.deltaTime;
        if(_elapsedTime >= timeBetweenPoints) {
            _elapsedTime = 0f;
            transform.position = startPosition.position;
        }

        // Moves
        float timeDelta = _elapsedTime / timeBetweenPoints;
        transform.position = Vector3.Lerp(startPosition.position, endPosition.position, timeDelta);
    }

    private void OnCollisionEnter(Collision collision) {
        if(collision.gameObject.CompareTag("Player")) {
            collision.gameObject.GetComponent<PlayerRagdoll>().StunPlayer(Vector3.zero, 1);
        }
    }
}
