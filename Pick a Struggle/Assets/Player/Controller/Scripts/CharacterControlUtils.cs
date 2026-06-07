using UnityEngine;

public static class CharacterControlUtils
{
    public static Vector3 GetNormalWithSphereCast(CharacterController characterController, LayerMask layermask = default) {
        Vector3 normal = Vector3.zero;
        Vector3 center = characterController.transform.position + characterController.center;
        float distance = characterController.height / 2f + characterController.stepOffset;

        RaycastHit hit;
        if(Physics.SphereCast(center, characterController.radius, Vector3.down, out hit, distance, layermask)) {
            normal = hit.normal;
        }

        return normal;
    }
}
