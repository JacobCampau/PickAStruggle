using PurrNet;
using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

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

    [Header("Player's model container")]
    [SerializeField] private Transform _characterModel;

    // Getting up logic
    private bool _isFacingUp;

    private enum EAnimationState
    {
        standingUp,
        resetingBones,
        complete
    }
    private EAnimationState _animationState;

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
        _rootBone = _hipBone.parent;

        _bones = _rootBone.GetComponentsInChildren<Transform>();

        // Initializations
        _animationState = EAnimationState.complete;
    }

    private void Start() 
    {
        SetIk(1);
        SetTargetTracking(1);

        if (_slowDown)
            Time.timeScale = 0.333f;
    }

    private void Update(){
        SetActiveEye();
        RagdollStandup();
    }

    // Eye control
    private void SetActiveEye() 
    {
        if (_handler.playerState == PlayerHandler.EPlayerState.ragdoll)
        {
            SetStunEyes();
        }
        else if (_handler.playerState == PlayerHandler.EPlayerState.dead)
        {
            SetDeadEyes();
        }
        else
        {
            SetNormalEyes();
        }
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
    private void SetIk(float amount)
    {
        foreach (TwoBoneIKConstraint constraint in _ikConstraints)
            constraint.weight = amount;
    }

    private void SetTargetTracking(float amount)
    {
        _targetTracker.weight = amount;
    }

    // Ragdoll stunning
    public void StunPlayer(bool getBackUp, Vector3 force, float mult) {
        // Make sure GetUp isnt running
        CancelInvoke(nameof(GetUp));

        // Begin ragdoll process and timer
        if (getBackUp)
            Invoke(nameof(GetUp), _stunTimer);
        
        if(_debug)
            Debug.Log("Ragdoll applied force: " + force);

        _ragdoll.EnableRagdoll(mult * force / _handler.RB.mass);
        SetTargetTracking(0);
        SetIk(0);
    }

    private void GetUp()
    {
        AlignRotation();
        _ragdoll.EnableAnimator();
        AlignPosition();
    }

    public void ExitRagdoll()
    {
        _animationState = EAnimationState.resetingBones;
        _handler.playerState = PlayerHandler.EPlayerState.animated;
    }

    private void ResetingBones() {
        _anim.enabled = true;
    }

    private void StandingUp()
    {

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
        _characterModel.localPosition = Vector3.zero;
        _characterModel.localRotation = Quaternion.identity;
    }

    private void AlignRotation()
    {
        // Direction of fall
        _isFacingUp = _bodyBone.forward.y > 0;

        Quaternion desiredDirection = Quaternion.Euler(0f, _bodyBone.eulerAngles.y, 0f);
        transform.rotation = desiredDirection;

        if (_debug)
        {
            Debug.Log($"Start Direction: {_bodyBone.rotation.eulerAngles}");
            Debug.Log($"Specific y direction: {_bodyBone.rotation.eulerAngles.y}");
            Debug.Log($"Desired Direction: {desiredDirection.eulerAngles}");
            Debug.Log($"End Direction: {transform.rotation.eulerAngles}");
        }

        // Immediately face the opposite direction (saved incase needed for another time)
        //transform.rotation = Quaternion.LookRotation(-transform.forward, Vector3.up);
    }

    // Eye functions
    private void SetNormalEyes() {
        _normalEyes.SetActive(true);
        _deadEyes.SetActive(false);
        _stunnedEyes.SetActive(false);
    }

    private void SetDeadEyes() {
        _normalEyes.SetActive(false);
        _deadEyes.SetActive(true);
        _stunnedEyes.SetActive(false);
    }

    private void SetStunEyes() {
        _normalEyes.SetActive(false);
        _deadEyes.SetActive(false);
        _stunnedEyes.SetActive(true);
    }
}
