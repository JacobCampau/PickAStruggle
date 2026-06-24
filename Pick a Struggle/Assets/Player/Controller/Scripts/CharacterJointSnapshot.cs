#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor utility to snapshot, remove, and restore CharacterJoint components.
/// Attach this to the same GameObject as the CharacterJoint, or use it as a standalone tool.
/// </summary>
public class CharacterJointSnapshot: MonoBehaviour {
    [System.Serializable]
    public class CharacterJointData {
        // Connected body
        public string connectedBodyPath;   // scene path to the connected Rigidbody's GameObject

        // Anchors
        public Vector3 anchor;
        public Vector3 connectedAnchor;
        public bool autoConfigureConnectedAnchor;

        // Axes
        public Vector3 axis;
        public Vector3 swingAxis;

        // Low/High Twist Limit
        public float lowTwistLimitLimit;
        public float lowTwistLimitBounciness;
        public float lowTwistLimitContactDistance;

        public float highTwistLimitLimit;
        public float highTwistLimitBounciness;
        public float highTwistLimitContactDistance;

        // Swing Limits
        public float swing1LimitLimit;
        public float swing1LimitBounciness;
        public float swing1LimitContactDistance;

        public float swing2LimitLimit;
        public float swing2LimitBounciness;
        public float swing2LimitContactDistance;

        // Drives
        public bool enableProjection;
        public float projectionDistance;
        public float projectionAngle;

        // Break forces
        public float breakForce;
        public float breakTorque;

        // Collision
        public bool enableCollision;
        public bool enablePreprocessing;

        // Mass scale
        public float massScale;
        public float connectedMassScale;
    }

    [Header("Snapshot Storage")]
    public CharacterJointData snapshot;
    public bool hasSnapshot = false;

    [ContextMenu("1. Capture Snapshot")]
    public void CaptureSnapshot() {
        CharacterJoint joint = GetComponent<CharacterJoint>();
        if(joint == null) {
            Debug.LogError("No CharacterJoint found on this GameObject.", this);
            return;
        }

        snapshot = new CharacterJointData();

        // Connected body
        snapshot.connectedBodyPath = joint.connectedBody != null
            ? GetScenePath(joint.connectedBody.gameObject)
            : string.Empty;

        // Anchors
        snapshot.anchor                       = joint.anchor;
        snapshot.connectedAnchor              = joint.connectedAnchor;
        snapshot.autoConfigureConnectedAnchor = joint.autoConfigureConnectedAnchor;

        // Axes
        snapshot.axis      = joint.axis;
        snapshot.swingAxis = joint.swingAxis;

        // Low Twist Limit
        snapshot.lowTwistLimitLimit         = joint.lowTwistLimit.limit;
        snapshot.lowTwistLimitBounciness    = joint.lowTwistLimit.bounciness;
        snapshot.lowTwistLimitContactDistance = joint.lowTwistLimit.contactDistance;

        // High Twist Limit
        snapshot.highTwistLimitLimit          = joint.highTwistLimit.limit;
        snapshot.highTwistLimitBounciness     = joint.highTwistLimit.bounciness;
        snapshot.highTwistLimitContactDistance = joint.highTwistLimit.contactDistance;

        // Swing 1 Limit
        snapshot.swing1LimitLimit           = joint.swing1Limit.limit;
        snapshot.swing1LimitBounciness      = joint.swing1Limit.bounciness;
        snapshot.swing1LimitContactDistance = joint.swing1Limit.contactDistance;

        // Swing 2 Limit
        snapshot.swing2LimitLimit           = joint.swing2Limit.limit;
        snapshot.swing2LimitBounciness      = joint.swing2Limit.bounciness;
        snapshot.swing2LimitContactDistance = joint.swing2Limit.contactDistance;

        // Projection
        snapshot.enableProjection   = joint.enableProjection;
        snapshot.projectionDistance = joint.projectionDistance;
        snapshot.projectionAngle    = joint.projectionAngle;

        // Break forces
        snapshot.breakForce  = joint.breakForce;
        snapshot.breakTorque = joint.breakTorque;

        // Flags
        snapshot.enableCollision    = joint.enableCollision;
        snapshot.enablePreprocessing = joint.enablePreprocessing;

        // Mass scales
        snapshot.massScale          = joint.massScale;
        snapshot.connectedMassScale = joint.connectedMassScale;

        hasSnapshot = true;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
        //Debug.Log($"[CharacterJointSnapshot] Snapshot captured on '{gameObject.name}'.", this);
    }

    [ContextMenu("2. Destroy Joint")]
    public void DestroyJoint() {
        CharacterJoint joint = GetComponent<CharacterJoint>();
        if(joint == null) {
            Debug.LogWarning("No CharacterJoint to destroy.", this);
            return;
        }

        if(!hasSnapshot)
            Debug.LogWarning("[CharacterJointSnapshot] Destroying joint WITHOUT a snapshot — restore will not be possible.", this);

#if UNITY_EDITOR
        Undo.DestroyObjectImmediate(joint);
#else
        Destroy(joint);
#endif
        //Debug.Log($"[CharacterJointSnapshot] CharacterJoint destroyed on '{gameObject.name}'.", this);
    }

    [ContextMenu("3. Restore Joint From Snapshot")]
    public void RestoreJoint() {
        if(!hasSnapshot || snapshot == null) {
            Debug.LogError("[CharacterJointSnapshot] No snapshot available. Capture one first.", this);
            return;
        }

        if(GetComponent<CharacterJoint>() != null) {
            Debug.LogWarning("[CharacterJointSnapshot] A CharacterJoint already exists. Remove it first.", this);
            return;
        }

#if UNITY_EDITOR
        CharacterJoint joint = Undo.AddComponent<CharacterJoint>(gameObject);
#else
        CharacterJoint joint = gameObject.AddComponent<CharacterJoint>();
#endif

        // Connected body
        if(!string.IsNullOrEmpty(snapshot.connectedBodyPath)) {
            GameObject connectedGO = GameObject.Find(snapshot.connectedBodyPath);
            if(connectedGO != null)
                joint.connectedBody = connectedGO.GetComponent<Rigidbody>();
            else
                Debug.LogWarning($"[CharacterJointSnapshot] Could not find connected body at path: '{snapshot.connectedBodyPath}'", this);
        }

        // autoConfigureConnectedAnchor must be set BEFORE anchor/connectedAnchor
        joint.autoConfigureConnectedAnchor = snapshot.autoConfigureConnectedAnchor;
        joint.anchor                       = snapshot.anchor;
        joint.connectedAnchor              = snapshot.connectedAnchor;

        // Axes
        joint.axis      = snapshot.axis;
        joint.swingAxis = snapshot.swingAxis;

        // Limits
        joint.lowTwistLimit  = new SoftJointLimit { limit = snapshot.lowTwistLimitLimit, bounciness = snapshot.lowTwistLimitBounciness, contactDistance = snapshot.lowTwistLimitContactDistance };
        joint.highTwistLimit = new SoftJointLimit { limit = snapshot.highTwistLimitLimit, bounciness = snapshot.highTwistLimitBounciness, contactDistance = snapshot.highTwistLimitContactDistance };
        joint.swing1Limit    = new SoftJointLimit { limit = snapshot.swing1LimitLimit, bounciness = snapshot.swing1LimitBounciness, contactDistance = snapshot.swing1LimitContactDistance };
        joint.swing2Limit    = new SoftJointLimit { limit = snapshot.swing2LimitLimit, bounciness = snapshot.swing2LimitBounciness, contactDistance = snapshot.swing2LimitContactDistance };

        // Projection
        joint.enableProjection   = snapshot.enableProjection;
        joint.projectionDistance = snapshot.projectionDistance;
        joint.projectionAngle    = snapshot.projectionAngle;

        // Break forces
        joint.breakForce  = snapshot.breakForce;
        joint.breakTorque = snapshot.breakTorque;

        // Flags
        joint.enableCollision     = snapshot.enableCollision;
        joint.enablePreprocessing = snapshot.enablePreprocessing;

        // Mass scales
        joint.massScale          = snapshot.massScale;
        joint.connectedMassScale = snapshot.connectedMassScale;

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
#endif
        //Debug.Log($"[CharacterJointSnapshot] CharacterJoint restored on '{gameObject.name}'.", this);
    }

    // --- Helpers ---

    private static string GetScenePath(GameObject go) {
        string path = go.name;
        Transform t = go.transform.parent;
        while(t != null) {
            path = t.name + "/" + path;
            t = t.parent;
        }
        return path;
    }

    private void Start() {
        CaptureSnapshot();
    }
}
#endif