using PurrNet;
using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UIElements;

public class PlayerAnimator : NetworkIdentity
{
    private enum EAnimationState {
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
    private float _timeToWakeUp;
    [SerializeField] private float _endRagdollSpeedThreshold;
    private bool _ragdollMoving;

    [Header("Needed Bones")]
    [SerializeField] private Transform _bodyBone;
    private Transform _hipBone;
    private Transform _rootBone;
    private Transform[] _bones;

    private BoneTransform[] _getUpFaceUpBoneTransforms;
    private BoneTransform[] _getUpFaceDownBoneTransforms;
    private BoneTransform[] _ragdollBoneTransforms;
    private float _elapsedResetBonesTime;

    [Header("Player's model container")]
    [SerializeField] private Transform _characterModel;

    // Getting up logic
    private bool _isFacingUp;
    private bool _bodyRigidbody;

    [Header("Getting up logic")]
    [SerializeField] private string _getUpFaceUpStateName;
    [SerializeField] private string _getUpFaceUpClipName;

    [SerializeField] private string _getUpFaceDownStateName;
    [SerializeField] private string _getUpFaceDownClipName;

    [SerializeField] private float _timeToResetBones;

    // FK & IK bool
    [Header("Ik controls")]
    [SerializeField] private TwoBoneIKConstraint[] _ikConstraints;
    [SerializeField] private MultiAimConstraint _targetTracker;
    [SerializeField] private Transform _targetLookAt;
    private Transform _restoreLookAt;
    private Transform _actualTarget;
    [SerializeField] private float _targetFollowSpeed;
    [SerializeField] private float _maxTargetDistance = 0f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem _fallenParticles;

    protected override void OnSpawned()
    {
        base.OnSpawned();
        enabled = isOwner;
    }

    private void Awake()
    {
        // Needed components
        _handler = GetComponent<PlayerHandler>();

        _bodyRigidbody = _bodyBone.GetComponent<Rigidbody>();

        _anim = GetComponentInChildren<Animator>();
        _ragdoll = GetComponent<RagdollLogic>();
        _combat = GetComponent<PlayerCombat>();
        _movement = GetComponent<PlayerMovement>();

        _hipBone = _bodyBone.parent;
        _rootBone = _hipBone.parent.parent; // Double parent since the hipbone has an origin transform as its parent

        // Bone transistions for ragdoll
        _bones = _rootBone.GetComponentsInChildren<Transform>();
        _getUpFaceUpBoneTransforms = new BoneTransform[_bones.Length];
        _getUpFaceDownBoneTransforms = new BoneTransform[_bones.Length];
        _ragdollBoneTransforms = new BoneTransform[_bones.Length];

        for (int i = 0; i < _bones.Length; i++) {
            _getUpFaceUpBoneTransforms[i] = new BoneTransform();
            _getUpFaceDownBoneTransforms[i] = new BoneTransform();
            _ragdollBoneTransforms[i] = new BoneTransform();
        }

        PopulateAnimationStartBoneTransforms(_getUpFaceUpClipName, _getUpFaceUpBoneTransforms);
        PopulateAnimationStartBoneTransforms(_getUpFaceDownClipName, _getUpFaceDownBoneTransforms);

        // Initializations
        _animationState = EAnimationState.complete;
        _timeToWakeUp = Random.Range(_stunTimer / 2f, _stunTimer * 1.5f);
        _restoreLookAt = _targetLookAt;
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
        MoveTargetPosition();

        switch (_handler.playerState)
        {
            case PlayerHandler.EPlayerState.moving:
                // Player controlled animations
                MovementAnimations();
                CombatAnimations();
                break;
            default:
                // Player NON controlled transition animations
                RagdollStandup();
                RagdollEnd();
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

    private void RagdollEnd(){
        if(_handler.playerState == PlayerHandler.EPlayerState.ragdoll){
            // In ragdoll, not dead
            if(_bodyRigidbody.linearVelocity.magnitude < _endRagdollSpeedThreshold){
                // player is no longer moving fast, so begin to wake up
                if(_ragdollMoving){
                    _ragdollMoving = false;
                    Invoke(nameof(GetUp), _timeToWakeUp);
                }
            }else{
                _ragdollMoving = true;
                CancelInvoke(nameof(GetUp));
            }
        }

        // If the player dies during the ragdoll, then prevent the getup function from being called
        if(_handler.playerState == PlayerHandler.EPlayerState.ragdoll){
            CancelInvoke(nameof(GetUp));
        }

        // If the player is in the process of getting up and is moved for any reason, reenter the ragdoll state
        if(_animationState == EAnimationState.resetingBones || _animationState == EAnimationState.standingUp){
            if(_handler.RB.linearVelocity > _endRagdollSpeedThreshold){
                // The player is in the process of getting up and has begun moving
                StunPlayer(_handler.RB.linearVelocity, 1);
            }
        }
        
    }

    public void FallParticles(Transform location){
        // Particle stuffs
    }

    // Player controlled animation triggers/bools
    private void MovementAnimations(){
        switch(_movement.moveState){
            case _movement.EPlayerMoveState.walking:
                if(_handler.RB.linearVelocity.magnitude <= 0.1f)
                    _anim.SetBool("isIdle", true);
                else
                    _anim.SetBool("isWalking", true);
                break;
            case _movement.EPlayerMoveState.sprinting:
                _anim.SetBool("isSprinting", true);
                break;
            case _movement.EPlayerMoveState.crouching:
                _anim.SetBool("isCrouching", true);
                break;
            default: // Tpose
                _anim.SetBool("Tpose", true);
                break;
        }
    }

    private void CombatAnimations(){
        switch(_combat.combatState){
            case _combat.EPlayerCombatState.emptyHanded:
                _anim.SetBool("emptyHanded", true);
                break;
            case _combat.EPlayerCombatState.oneHanded:
                _anim.SetBool("oneHanded", true);
                break;
            case _combat.EPlayerCombatState.twoHanded:
                _anim.SetBool("twoHanded", true);
                break;
            default:
                _anim.SetBool("emptyHanded", true);
                break;
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
    public void StunPlayer(Vector3 force, float mult) 
    {
        // Make sure GetUp isnt running
        CancelInvoke(nameof(GetUp));
        
        if(_debug)
            Debug.Log("Ragdoll applied force: " + force);

        _ragdoll.EnableRagdoll(mult * force / _handler.RB.mass);
    }

    private void GetUp()
    {
        // Is the player on its back?
        _isFacingUp = _bodyBone.forward.y > 0;

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

        BoneTransform[] standUpBoneTransforms = GetStandUpBoneTransforms();

        for (int i = 0; i < _bones.Length; i++)
        {
            _bones[i].localPosition = Vector3.Lerp(
                _ragdollBoneTransforms[i].Position,
                standUpBoneTransforms[i].Position,
                elapsedPercentage);

            _bones[i].localRotation = Quaternion.Lerp(
                _ragdollBoneTransforms[i].Rotation,
                standUpBoneTransforms[i].Rotation,
                elapsedPercentage);
        }

        if (elapsedPercentage >= 1)
        {
            _animationState = EAnimationState.standingUp;
            _anim.enabled = true;
            _anim.Play(GetStandUpStateName(), 0, 0);
        }
    }

    private void StandingUp()
    {
        if (_anim.GetCurrentAnimatorStateInfo(0).IsName(_getUpFaceUpStateName) == false)
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

        foreach (AnimationClip clip in _anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == clipName)
            {
                clip.SampleAnimation(_anim.gameObject, 0);
                PopulateBoneTransforms(boneTransforms);
                break;
            }
        }

        transform.position = positionBeforeSampling;
        transform.rotation = rotationBeforeSampling;
    }

    private string GetStandUpStateName()
    {
        return _isFacingUp ? _getUpFaceUpStateName : _getUpFaceDownStateName;
    }

    private BoneTransform[] GetStandUpBoneTransforms()
    {
        return _isFacingUp ? _getUpFaceUpBoneTransforms : _getUpFaceDownBoneTransforms;
    }

    // Eye functions
    public void SetLookAtObject(Transform object){
        Vector3 distance = object.position - transform.position;
        if(distance.magnitude < _maxTargetDistance){
            _actualTarget = object;
        }
    }

    private MoveTargetPosition(){
        if(_actualTarget != null){
            Vector3 distance = _actualTarget - transform.position;
            if(distance.magnitude >= _maxTargetDistance){
                // move target to default
                _targetLookAt.position = Vector3.Lerp(_targetLookAt.position, _restoreLookAt.position, Time.deltaTime * _targetFollowSpeed);
            }else{
                // move target to target
                _targetLookAt.position = Vector3.Lerp(_targetLookAt.position, _actualTarget.position, Time.deltaTime * _targetFollowSpeed);
            }
        }
    }

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
