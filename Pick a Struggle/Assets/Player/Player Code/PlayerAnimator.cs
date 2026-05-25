using PurrNet;
using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public class PlayerAnimator : NetworkIdentity
{
    // Debug
    [SerializeField] private bool _debug;
    [SerializeField] private bool _slowDown;

    // Handler
    private PlayerHandler _handler;

    // Needed component calls
    private Animator _anim;
    private RagdollLogic _ragdoll;
    private PlayerCombat _combat;
    private PlayerMovement _movement;

    [Header("Eyes")]
    [SerializeField] private GameObject _normalEyes;
    [SerializeField] private GameObject _deadEyes;
    [SerializeField] private GameObject _stunnedEyes;

    [Header("Time in ragdoll")]
    [SerializeField] private float _stunTimer;

    [Header("Needed Bones")]
    [SerializeField] private Transform _bodyBone;
    private Transform _hipBone;
    private Transform _rootBone;
    private Transform[] _bones;

    private BoneTransform[] _standUpBoneTransforms;
    private BoneTransform[] _ragdollBoneTransforms;
    private float _elapsedResetBonesTime;

    [Header("Player's model container")]
    [SerializeField] private Transform _characterModel;

    // Getting up logic
    private bool _isFacingUp;
    private int _hipIndex;

    private enum EAnimationState
    {
        standingUp,
        resetingBones,
        complete
    }
    private EAnimationState _animationState;

    private class BoneTransform
    {
        public Vector3 Position { get; set; }

        public Quaternion Rotation { get; set; }
    }

    [Header("Getting up logic")]
    [SerializeField] private string _standUpStateName;
    [SerializeField] private string _standUpClipName;
    [SerializeField] private float _timeToResetBones;

    // FK & IK bool
    [Header("Ik controls")]
    [SerializeField] private TwoBoneIKConstraint[] _ikConstraints;
    [SerializeField] private MultiAimConstraint _targetTracker;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;
    }

    private void Awake()
    {
        // Needed components
        _handler = GetComponent<PlayerHandler>();

        _anim = GetComponentInChildren<Animator>();
        _ragdoll = GetComponent<RagdollLogic>();
        _combat = GetComponent<PlayerCombat>();
        _movement = GetComponent<PlayerMovement>();

        _hipBone = _bodyBone.parent;
        _rootBone = _hipBone.parent.parent; // Double parent since the hipbone has an origin transform as its parent

        // Bone transistions for ragdoll
        _bones = _rootBone.GetComponentsInChildren<Transform>();
        _standUpBoneTransforms = new BoneTransform[_bones.Length];
        _ragdollBoneTransforms = new BoneTransform[_bones.Length];

        for (int i = 0; i < _bones.Length; i++) {
            _standUpBoneTransforms[i] = new BoneTransform();
            _ragdollBoneTransforms[i] = new BoneTransform();

            if(_bones[i] == _hipBone) {
                _hipIndex = i;
            }
        }

        PopulateAnimationStartBoneTransforms(_standUpClipName, _standUpBoneTransforms);

        for(int i = 0; i < _bones.Length; i++) {
            if(_bones[i] == _hipBone) {
                Debug.Log($"Final position for hip: {_standUpBoneTransforms[i].Position}");
                Debug.Log($"Final rotation for hip: {_standUpBoneTransforms[i].Rotation.eulerAngles}");
            }
        }

        // Initializations
        _animationState = EAnimationState.complete;
    }

    private void Start() 
    {
        SetIk(1);
        SetTargetTracking(0); // Not set yet

        if (_slowDown)
            Time.timeScale = 0.333f;
    }

    private void Update()
    {
        SetActiveEye();

        switch (_handler.playerState)
        {
            case PlayerHandler.EPlayerState.moving:
                // Player controlled animations
                break;
            default:
                // Player NON controlled transition animations
                RagdollStandup();
                break;
        }
    }

    // Eye control
    private void SetActiveEye() 
    {
        if (_handler.playerState == PlayerHandler.EPlayerState.ragdoll)
            SetStunEyes();
        else if (_handler.playerState == PlayerHandler.EPlayerState.dead)
            SetDeadEyes();
        else
            SetNormalEyes();
    }

    // Ragdoll control
    private void RagdollStandup()
    {
        if (_animationState == EAnimationState.resetingBones)
        {
            // Reset player bones after a fall to the first frame of the needed animation
            ResetingBones();
        }
        else if (_animationState == EAnimationState.standingUp)
        {
            // Play the standup animation
            StandingUp();
        }
    }

    // FK & IK Transisitons
    public void SetIk(float amount)
    {
        foreach (TwoBoneIKConstraint constraint in _ikConstraints)
            constraint.weight = amount;
    }

    public void SetTargetTracking(float amount)
    {
        _targetTracker.weight = amount;
    }

    // Ragdoll stunning
    public void StunPlayer(bool getBackUp, Vector3 force, float mult) 
    {
        // Make sure GetUp isnt running
        CancelInvoke(nameof(GetUp));

        // Begin ragdoll process and timer
        if (getBackUp)
            Invoke(nameof(GetUp), _stunTimer);
        
        if(_debug)
            Debug.Log("Ragdoll applied force: " + force);

        _ragdoll.EnableRagdoll(mult * force / _handler.RB.mass);
    }

    private void GetUp()
    {
        // Align player and turn off the rigidbody
        AlignRotation();
        _ragdoll.EnableAnimator();
        AlignPosition();

        // Get initial transforms
        PopulateBoneTransforms(_ragdollBoneTransforms);
        _elapsedResetBonesTime = 0;

        // Change states
        _handler.playerState = PlayerHandler.EPlayerState.animated;
        _animationState = EAnimationState.resetingBones;
    }

    private void ResetingBones() 
    {
        // Lerp the animated transforms
        _elapsedResetBonesTime += Time.deltaTime;
        float elapsedPercentage = _elapsedResetBonesTime / _timeToResetBones;

        for (int i = 0; i < _bones.Length; i++)
        {
            _bones[i].localPosition = Vector3.Lerp(
                _ragdollBoneTransforms[i].Position,
                _standUpBoneTransforms[i].Position,
                elapsedPercentage);

            _bones[i].localRotation = Quaternion.Lerp(
                _ragdollBoneTransforms[i].Rotation,
                _standUpBoneTransforms[i].Rotation,
                elapsedPercentage);
        }

        if (elapsedPercentage >= 1)
        {
            _animationState = EAnimationState.standingUp;
            _anim.enabled = true;
            _anim.Play(_standUpStateName);
        }
    }

    private void StandingUp()
    {
        if (_anim.GetCurrentAnimatorStateInfo(0).IsName(_standUpStateName) == false)
        {
            _animationState = EAnimationState.complete;
            _handler.playerState = PlayerHandler.EPlayerState.moving;
        }
    }

    // Aligning player
    private void AlignPosition()
    {
        // Initial movement
        Vector3 bodyPosition = _bodyBone.position;
        transform.position = new Vector3(bodyPosition.x, transform.position.y, bodyPosition.z);

        // Adjust if needed
        Vector3 rayOrigin = new Vector3(transform.position.x, bodyPosition.y + 1, transform.position.z);
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, 10))
            transform.position = new Vector3(bodyPosition.x, hitInfo.point.y, bodyPosition.z);

        // Reset timmy
        Vector3 moveTimmy = Vector3.zero - _characterModel.localPosition;
        _characterModel.localPosition += moveTimmy;

        // Move hipbone
        Vector3 moveHip = _bodyBone.transform.localPosition - _hipBone.transform.localPosition;
        _hipBone.transform.localPosition += moveHip;
        foreach(Transform child in _hipBone.transform) {
            child.transform.localPosition -= moveHip;
        }
        _hipBone.localPosition = new Vector3(0f, _hipBone.localPosition.y - moveTimmy.y, 0f);
    }

    private void AlignRotation()
    {
        // Direction of fall
        _isFacingUp = _bodyBone.forward.y > 0;

        // Rotate timmy
        Quaternion desiredDirection = Quaternion.Euler(0f, _bodyBone.eulerAngles.y, 0f);
        Quaternion delta = desiredDirection * Quaternion.Inverse(_characterModel.rotation);
        _characterModel.rotation = desiredDirection;

        // Rotate the hip
        _hipBone.rotation *= Quaternion.Inverse(delta);

        // Immediately face the opposite direction (saved in case it is needed for another time)
        //transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
    }

    private void PopulateBoneTransforms(BoneTransform[] boneTransforms)
    {
        for (int i = 0; i < _bones.Length; i++)
        {
            boneTransforms[i].Position = _bones[i].localPosition;
            boneTransforms[i].Rotation = _bones[i].localRotation;
        }
    }

    private void PopulateAnimationStartBoneTransforms(string clipName, BoneTransform[] boneTransforms)
    {
        Vector3 positionBeforeSampling = transform.position;
        Quaternion rotationBeforeSampling = transform.rotation;

        bool found = false;
        foreach (AnimationClip clip in _anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                found = true;
                clip.SampleAnimation(_anim.gameObject, 0);
                PopulateBoneTransforms(boneTransforms);
                break;
            }
        }

        if(!found)
            Debug.LogError($"Clip '{clipName}' not found! Bones were never sampled.");

        transform.position = positionBeforeSampling;
        transform.rotation = rotationBeforeSampling;
    }

    // Eye functions
    private void SetNormalEyes() 
    {
        _normalEyes.SetActive(true);
        _deadEyes.SetActive(false);
        _stunnedEyes.SetActive(false);
    }

    private void SetDeadEyes() 
    {
        _normalEyes.SetActive(false);
        _deadEyes.SetActive(true);
        _stunnedEyes.SetActive(false);
    }

    private void SetStunEyes() 
    {
        _normalEyes.SetActive(false);
        _deadEyes.SetActive(false);
        _stunnedEyes.SetActive(true);
    }
}
