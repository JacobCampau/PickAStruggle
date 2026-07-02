using UnityEngine;

public class RagdollPart : MonoBehaviour
{
    [field: SerializeField] public Transform parentObject { get; private set; }

    void Start()
    {
        parentObject = transform.parent;
    }

    public void RestoreParent()
    {
        transform.SetParent(parentObject);
    }
}
