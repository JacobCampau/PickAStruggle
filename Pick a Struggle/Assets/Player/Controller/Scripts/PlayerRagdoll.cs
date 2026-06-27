using System.Globalization;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.WSA;

public class PlayerRagdoll : MonoBehaviour
{
    private PlayerState _playerState;
    private Animator _anim; 
    private Rigidbody[] _rigidbodies;
    private CharacterJoint[] _joints;
    private Collider[] _colliders;
    private Rigidbody _rbBody;

    [Header("Break Apart")]
    [SerializeField] private float _directionMult = 1f;

    [Header("Camera")]
    [SerializeField] private Transform _cameraContainer;
    [SerializeField] private float _cameraFollowTimer;
    [SerializeField] private Transform _bodyFollowTransform;
    [SerializeField] private Transform _headFollowTransform;

    [Header("Needed Transforms")]
    [SerializeField] private Transform _ragdollRoot;
    [SerializeField] private Transform _bodyBone;

    [Header("Ragdoll Weight")]
    public float weight;

    [Header("Getting up logic")]
    [SerializeField] private float _wakeUpTimer = 3f;
    [SerializeField] private float _timerRange = 1f;
    [SerializeField] private float _endRagdollSpeedThreshold = 0.1f;
    [Space]
    [SerializeField] private string _getUpFaceUpStateName;
    [SerializeField] private string _getUpFaceUpClipName;

    [SerializeField] private string _getUpFaceDownStateName;
    [SerializeField] private string _getUpFaceDownClipName;

    [SerializeField] private string _fallingStateName;
    [SerializeField] private string _fallingClipName;

    [SerializeField] private float _timeToResetBones;

    [Header("Broken Adjustments")]
    [SerializeField] private float _timeToRepairBones;
    [SerializeField] private float _repairVariations;
    [SerializeField] private float _repairVerticalOffset;
    
    [Header("Particles")]
    [SerializeField] private ParticleSystem _fallenParticles;

    private bool _isBroken = false;
    private bool _isFaceUp = false;
    private bool _isGettingUp = false;
    public bool RagdollIsActive { get; private set; } = false;

    private class BoneTransform {
        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }
    }
    private BoneTransform[] _getUpFaceUpBoneTransforms;
    private BoneTransform[] _getUpFaceDownBoneTransforms;
    private BoneTransform[] _ragdollBoneTransforms;
    private BoneTransform[] _fallingTransforms;

    private float _elapsedResetBonesTime = 0f;
    private float _currentFollowTime = 0f;
    private float[] _randomTimes;

    private Transform _hipBone;
    private Transform _rootBone;
    private Transform[] _bones;

    private Vector3 _initialCameraLocation;

    private void Awake() {
        _anim = GetComponentInChildren<Animator>();
        _playerState = GetComponentInChildren<PlayerState>();
        _rbBody = _bodyBone.GetComponent<Rigidbody>();

        _rigidbodies = _ragdollRoot.GetComponentsInChildren<Rigidbody>();
        _joints = _ragdollRoot.GetComponentsInChildren<CharacterJoint>();
        _colliders = _ragdollRoot.GetComponentsInChildren<Collider>();

        _initialCameraLocation = _cameraContainer.localPosition;
        _hipBone = _bodyBone.parent;
        _rootBone = _hipBone.parent.parent; // Double parent since the hipbone has an origin transform as its parent

        // Bone transistions for ragdoll
        _bones = _rootBone.GetComponentsInChildren<Transform>();
        _getUpFaceUpBoneTransforms = new BoneTransform[_bones.Length];
        _getUpFaceDownBoneTransforms = new BoneTransform[_bones.Length];
        _ragdollBoneTransforms = new BoneTransform[_bones.Length];
        _fallingTransforms = new BoneTransform[_bones.Length];
        _randomTimes = new float[_bones.Length];

        for(int i = 0; i < _bones.Length; i++) {
            _getUpFaceUpBoneTransforms[i] = new BoneTransform();
            _getUpFaceDownBoneTransforms[i] = new BoneTransform();
            _ragdollBoneTransforms[i] = new BoneTransform();
            _fallingTransforms[i] = new BoneTransform();
            _randomTimes[i] = 0f;
        }

        PopulateAnimationStartBoneTransforms(_getUpFaceUpClipName, _getUpFaceUpBoneTransforms);
        PopulateAnimationStartBoneTransforms(_getUpFaceDownClipName, _getUpFaceDownBoneTransforms);
        PopulateAnimationStartBoneTransforms(_fallingStateName, _fallingTransforms);
    }

    private void Start() {
        // Starting state
        if(_playerState.CurrentPlayerMovementState == EPlayerMovementState.Ragdoll) {
            EnableRagdoll(Vector3.zero);
        } else {
            DisableRagdoll();
        }

        weight = 0;
        foreach(Rigidbody rb in _rigidbodies)
            weight += rb.mass;
    }

    private void Update() {
        RagdollEnd();

        if(_playerState.CurrentRagdollState == ERagdollState.ResetingBones) {
            ResetingBones();
        } else if(_playerState.CurrentRagdollState == ERagdollState.StandingUp) {
            StandingUp();
        }
    }

    private void LateUpdate() {
        CameraHandler();
    }

    void CameraHandler() {
        if(_playerState.CurrentRagdollState == ERagdollState.Active) {
            _currentFollowTime += Time.deltaTime;

            if(!_isBroken) {
                _cameraContainer.position = Vector3.Lerp(_cameraContainer.position, _bodyFollowTransform.position, _currentFollowTime/_cameraFollowTimer);
            } else {
                _cameraContainer.position = Vector3.Lerp(_cameraContainer.position, _headFollowTransform.position, _currentFollowTime/_cameraFollowTimer);
            }
        } else if(_playerState.CurrentRagdollState == ERagdollState.ResetingBones) {
            _currentFollowTime = 0;

            if(!_isBroken) {
                _cameraContainer.position = _bodyFollowTransform.position;
            } else {
                _cameraContainer.position = _headFollowTransform.position;
            }
        } else if(_playerState.CurrentRagdollState == ERagdollState.StandingUp) {
            _currentFollowTime += Time.deltaTime;
            _cameraContainer.localPosition = Vector3.Lerp(_cameraContainer.localPosition, _initialCameraLocation, _currentFollowTime/_cameraFollowTimer);
        } else if(_playerState.CurrentRagdollState == ERagdollState.Complete) {
            _currentFollowTime = 0;
            _cameraContainer.localPosition = _initialCameraLocation;
        }
    }

    private void RagdollEnd() {
        if(_playerState.CurrentPlayerMovementState == EPlayerMovementState.Ragdoll) {
            // In ragdoll, not dead
            if(_rbBody.linearVelocity.magnitude < _endRagdollSpeedThreshold) {
                // player is no longer moving fast, so begin to wake up
                if(!_isGettingUp) {
                    _isGettingUp = true;

                    float getUpTime = Random.Range(_wakeUpTimer - _timerRange, _wakeUpTimer + _timerRange);
                    Invoke(nameof(GetUp), getUpTime);
                }
            } else {
                _isGettingUp = false;
                CancelInvoke(nameof(GetUp));
            }
        }

        // If the player dies during the ragdoll, then prevent the getup function from being called
        if(_playerState.isDead) {
            CancelInvoke(nameof(GetUp));
            Debug.Log("DEAD RAGDOLL CANCEL");
        }

        // If the player is in the process of getting up and is moved for any reason, reenter the ragdoll state
        if(_playerState.CurrentRagdollState == ERagdollState.ResetingBones || _playerState.CurrentRagdollState == ERagdollState.StandingUp) {
            if(_rbBody.linearVelocity.magnitude > _endRagdollSpeedThreshold) {
                // The player is in the process of getting up and has begun moving
                StunPlayer(_rbBody.linearVelocity, 1);
                Debug.Log("Hit during get up");
            }
        }

    }

    private void GetUp() {
        // Is the player on its back?
        _isFaceUp = _bodyBone.forward.y > 0;

        // Align player and turn off the rigidbody
        AlignPosition();
        AlignRotation();
        EnableAnimator();

        // Get initial transforms
        PopulateBoneTransforms(_ragdollBoneTransforms);
        _elapsedResetBonesTime = 0;

        // Assign random times incase of a bone repair
        for(int i = 0; i < _bones.Length; i++)
            _randomTimes[i] = Random.Range(_timeToRepairBones - _repairVariations, _timeToRepairBones + _repairVariations);
       
        // Change states
        _playerState.SetPlayerRagdollState(ERagdollState.ResetingBones);
    }

    public void BreakPlayer(Vector3 direction, float mult) {
        // Make sure GetUp isnt running
        CancelInvoke(nameof(GetUp));

        _isBroken = true;

        ActivateIndividualRagdolls(direction * mult);
    }

    private void ActivateIndividualRagdolls(Vector3 dir) {
        _playerState.SetPlayerRagdollState(ERagdollState.Active);

        // Activate the components
        _anim.enabled = false;
        foreach(CharacterJoint joint in _joints) {
            Destroy(joint.GetComponent<CharacterJoint>());
        }

        foreach(Collider collider in _colliders)
            collider.isTrigger = false;

        foreach(Rigidbody rb in _rigidbodies) {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.detectCollisions = true;
            rb.useGravity = true;
        }

        // Apply the directions to each body part
        foreach(Rigidbody bone in _rigidbodies) {
            // Get direction from body
            Vector3 directionFromBody = bone.position - _rbBody.position;
            Vector3 force = dir + (directionFromBody * _directionMult);
            float proportionalMult = bone.mass / weight;

            // Toss
            TossRagdoll(bone, force, proportionalMult);
        }
    }

    public void StunPlayer(Vector3 force, float mult) {
        // Make sure GetUp isnt running
        CancelInvoke(nameof(GetUp));

        EnableRagdoll(mult * force.normalized);
    }

    public void EnableRagdoll(Vector3 force) {
        _playerState.SetPlayerRagdollState(ERagdollState.Active);

        // Activate the components
        _anim.enabled = false;
        foreach(CharacterJoint joint in _joints)
            joint.enableCollision = true;

        foreach(Collider collider in _colliders)
            collider.isTrigger = false;

        foreach(Rigidbody rb in _rigidbodies) {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.detectCollisions = true;
            rb.useGravity = true;
        }

        // Apply force direction
        TossRagdoll(_rbBody, force, 1);
    }

    private void DisableRagdoll() {
        foreach(CharacterJoint joint in _joints)
            joint.enableCollision = false;

        foreach(Collider collider in _colliders)
            collider.isTrigger = true;

        foreach(Rigidbody rb in _rigidbodies) {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.useGravity = false;
        }
    }

    public void EnableAnimator() {
        // Reapply any missing joints
        foreach(Transform bone in _bones) {
            CharacterJointSnapshot snapshot = bone.GetComponent<CharacterJointSnapshot>();
            if(snapshot != null)
                snapshot.RestoreJoint();
        }
        _joints = _ragdollRoot.GetComponentsInChildren<CharacterJoint>();

        // Reset body parts
        DisableRagdoll();
    }

    private void ResetingBones() {
        // Lerp the animated transforms
        _elapsedResetBonesTime += Time.deltaTime;
        float elapsedPercentage = _elapsedResetBonesTime / _timeToResetBones;

        BoneTransform[] standUpBoneTransforms = GetStandUpBoneTransforms();

        for(int i = 0; i < _bones.Length; i++) {
            if(_isBroken) {
                elapsedPercentage = _elapsedResetBonesTime / _randomTimes[i];

                _bones[i].localPosition = Vector3.Lerp(
                    _ragdollBoneTransforms[i].Position,
                    standUpBoneTransforms[i].Position,
                    elapsedPercentage);

                _bones[i].localRotation = Quaternion.Lerp(
                    _ragdollBoneTransforms[i].Rotation,
                    standUpBoneTransforms[i].Rotation,
                    elapsedPercentage);
            } else {
                _bones[i].localPosition = Vector3.Lerp(
                    _ragdollBoneTransforms[i].Position,
                    standUpBoneTransforms[i].Position,
                    elapsedPercentage);

                _bones[i].localRotation = Quaternion.Lerp(
                    _ragdollBoneTransforms[i].Rotation,
                    standUpBoneTransforms[i].Rotation,
                    elapsedPercentage);
            }
        }

        if(_isBroken ? _elapsedResetBonesTime > _randomTimes.Max() : elapsedPercentage >= 1) {
            _playerState.SetPlayerRagdollState(ERagdollState.StandingUp);
            _anim.enabled = true;
            _anim.Play(GetStandUpStateName(), 2, 0);
        }
    }

    private void StandingUp() {
        if(_anim.GetCurrentAnimatorStateInfo(2).IsName(GetStandUpStateName()) == false) {
            _playerState.SetPlayerRagdollState(ERagdollState.Complete);
            _isGettingUp = false;
            _isBroken = false;
        }
    }

    // Aligning player
    private void AlignPosition() {
        // Get vector difference from transform to body
        Vector3 bodyPosition = _bodyBone.position;
        Vector3 bodyToControllerDistance = new Vector3(bodyPosition.x - transform.position.x, 0f, bodyPosition.z - transform.position.z);
        Vector3 bodyGroundCheckPosition = new Vector3(bodyPosition.x, bodyPosition.y + 1, bodyPosition.z);

        float bodyToGroundDistance = 0f;

        if(Physics.Raycast(bodyGroundCheckPosition, Vector3.down, out RaycastHit hitInfo, 10)) {
            bodyToControllerDistance.y = hitInfo.point.y - transform.position.y;
            bodyToGroundDistance = _bodyBone.position.y - hitInfo.point.y;
        }

        // center body and move controller
        transform.position += bodyToControllerDistance;
        Vector3 moveBody = _hipBone.position - _bodyBone.position;
        foreach(Transform child in _hipBone.transform) {
            child.transform.position += moveBody;
        }

        // hip adjustment
        Vector3 moveHip = _hipBone.position - bodyPosition;
        _hipBone.position = new Vector3(_hipBone.position.x, _hipBone.position.y - moveHip.y, _hipBone.position.z);

        // adjust bones if broken
        if(_isBroken) {
            Vector3 adjustment = new Vector3(0f, _repairVerticalOffset, 0f);
            // move player up
            transform.position += adjustment;

            // move bones down
            foreach(Transform child in _hipBone.transform) {
                child.transform.position -= adjustment;
            }
        }
    }

    private void AlignRotation() {
        // Rotate timmy
        Quaternion desiredDirection = Quaternion.Euler(0f, _bodyBone.rotation.eulerAngles.y, 0f);
        Quaternion delta = desiredDirection * Quaternion.Inverse(transform.rotation);
        transform.rotation = desiredDirection;

        // Rotate the hip
        _hipBone.rotation *= Quaternion.Inverse(delta);

        // Immediately face the opposite direction (saved in case it is needed for another time)
        //transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up)
    }

    private void PopulateBoneTransforms(BoneTransform[] boneTransforms) {
        for(int i = 0; i < _bones.Length; i++) {
            boneTransforms[i].Position = _bones[i].localPosition;
            boneTransforms[i].Rotation = _bones[i].localRotation;
        }
    }

    private void SetBoneTransforms(BoneTransform[] boneTransforms) {
        for(int i = 0; i < _bones.Length; i++) {
            _bones[i].localPosition = boneTransforms[i].Position;
            _bones[i].localRotation = boneTransforms[i].Rotation;
        }
    }

    private void PopulateAnimationStartBoneTransforms(string clipName, BoneTransform[] boneTransforms) {
        BoneTransform[] initials;
        initials = new BoneTransform[_bones.Length];

        for(int i = 0; i < _bones.Length; i++) {
            initials[i] = new BoneTransform();
        }

        PopulateBoneTransforms(initials);

        foreach(AnimationClip clip in _anim.runtimeAnimatorController.animationClips) {
            if(clip.name == clipName) {
                clip.SampleAnimation(gameObject, 0);
                PopulateBoneTransforms(boneTransforms);
                break;
            }
        }

        SetBoneTransforms(initials);
    }

    private string GetStandUpStateName() {
        return _isBroken? _fallingStateName : _isFaceUp ? _getUpFaceUpStateName : _getUpFaceDownStateName;
    }

    private BoneTransform[] GetStandUpBoneTransforms() {
        return _isBroken? _fallingTransforms : _isFaceUp ? _getUpFaceUpBoneTransforms : _getUpFaceDownBoneTransforms;
    }

    public void TossRagdoll(Rigidbody bone, Vector3 dir, float mult) {
        Vector3 force = dir * mult;

        if(_playerState.CurrentRagdollState != ERagdollState.Complete)
            bone.AddForce(force * weight, ForceMode.Impulse);
    }
}