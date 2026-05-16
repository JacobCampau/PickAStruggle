using PurrNet;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Item : MonoBehaviour
{
    public ItemData data;
    
    private Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
    }
    
    public void StoreInHand(Transform parent) {
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        transform.SetParent(parent);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        gameObject.SetActive(false); // hidden until it is the active slot
    }

    public void Eject(Vector3 throwDir, float force = 0f)
    {
        transform.SetParent(null);
        gameObject.SetActive(true);

        rb.isKinematic = false;

        if (force > 0f)
            rb.AddForce(throwDir.normalized * force, ForceMode.Impulse);
    }
}
