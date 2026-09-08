using UnityEngine;

public class DebugCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision) {
        Debug.Log(collision.gameObject);
    }
}
