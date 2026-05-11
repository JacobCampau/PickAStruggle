using UnityEngine;
using PurrNet;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public class PlayerMovement : NetworkIdentity
{
    public PlayerStats stats;
    private RagdollLogic ragdoll;
    private PlayerAnimatior animator; 

    // Move stats
    private float moveSpeed;
    [SerializeField] private float sprintSpeedMult = 1.25f;
    [SerializeField] private float crouchSpeedMult = 0.5f;
    private float staminaMax = 100;
    private float staminaDrain = 5;
    private float currStamina;
    private float jumpForce;

    private float boostSpeed;
    private float boostStaminaDrain;

    private float totalSpeed;
    private float totalStaminaDrain;

    // Movement stats (same for all characters)
    [Header("Debug")]
    public bool debug;

    [Header("Movement")]
    [SerializeField] private float groundDrag;
    [SerializeField] private float jumpCooldown;
    [SerializeField] private float airMult;
    private bool readyToJump;

    [Header("Key Binds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    [SerializeField] private LayerMask whatIsGround;
    private bool grounded;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Stunned Timing")]
    [SerializeField] private float maxVelocity;

    // Stunned info
    [SerializeField] private float stunTimer;

    [Header("Other")]
    [SerializeField] private Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDir;

    private Rigidbody rb;

    public enum MovementState { 
        walking, sprinting, air, crouch
    }
    public MovementState state;

    protected override void OnSpawned() {
        base.OnSpawned();
        enabled = isOwner;
    }

    private void Start() {
        // Set starting values
        moveSpeed = stats.speed;
        staminaMax = stats.staminaMax;
        staminaDrain = stats.staminaDrain;
        currStamina = staminaMax;
        jumpForce = stats.jumpForce;

        // Call the setters
        SetMoveSpeed();
        SetStaminaDrain();

        // Other Set-up calls
        rb = GetComponent<Rigidbody>();
        ragdoll = GetComponent<RagdollLogic>();
        animator = GetComponent<PlayerAnimatior>();

        readyToJump = true;
    }

    private void Update() {
        // Always running, no matter frames
        StateHandler();
        SetStaminaDrain();

        // Ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, 0.2f, whatIsGround);
        if(debug)
            if(grounded) {
                Debug.DrawRay(transform.position, Vector3.down * 0.2f, Color.green);
            } else { 
                Debug.DrawRay(transform.position, Vector3.down * 0.2f, Color.red);
            }

        // Jump
        if(Input.GetKey(jumpKey) && grounded && readyToJump) {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Drag
        if(grounded) {
            rb.linearDamping = groundDrag;
        } else {
            rb.linearDamping = 0;
        }

        // Stamina Drain
        if(state == MovementState.sprinting){
            // Stamina drain
            currStamina -= totalStaminaDrain * Time.deltaTime;
            if(currStamina <= 0){ 
                currStamina = 0;
            }
        }else{
            // Regen stamina at twice the pace
            if(currStamina < staminaMax)
                currStamina += totalStaminaDrain * 2 * Time.deltaTime;
            
            if(currStamina > staminaMax){ 
                currStamina = staminaMax;
            }
        }

        // Hitting the ground too hard
        if(FallingFast() && grounded) {
            if(!ragdoll.ragdollActive) {
                if(debug)
                    Debug.Log("Ouch");
                StunPlayer();
            }
        }
    }

    private void FixedUpdate() {
        // Movement
        if(!ragdoll.ragdollActive)
            MovePlayer();
    }

    private Vector3 GetMovementDir() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        return orientation.forward * verticalInput + orientation.right * horizontalInput;
    }

    private void StateHandler() {
        if(grounded && Input.GetKey(crouchKey)){
            // Crouching
            state = MovementState.crouch;
            SetCrouchSpeed();
        } else if(grounded && Input.GetKey(sprintKey) && currStamina > 0) {
            // Sprinting
            state = MovementState.sprinting;
            SetSprintSpeed();
        } else if(grounded) {
            // Walking
            state = MovementState.walking;
            SetMoveSpeed();
        } else {
            // Air
            state = MovementState.air;
        }
    }

    private void MovePlayer() {
        // calc move direction
        moveDir = GetMovementDir();

        // On slope
        if(OnSlope() && !exitingSlope) {
            rb.AddForce(GetSlopeMoveDirection() * totalSpeed * 20f, ForceMode.Force);

            if(rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        if(grounded) {
            rb.AddForce(moveDir.normalized * totalSpeed * 10f, ForceMode.Force);
        } else {
            rb.AddForce(moveDir.normalized * totalSpeed * 10f * airMult, ForceMode.Force);
        }

        // Clamp speeds as needed
        SpeedControl();

        // Cancel gravity on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl() {
        if(OnSlope() && !exitingSlope) {
            // Limiting speed on slopes
            if(rb.linearVelocity.magnitude > totalSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * totalSpeed;
        } else {
            // Limiting speed on floor / air
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            if(flatVel.magnitude > totalSpeed) {
                Vector3 limitedVel = flatVel.normalized * totalSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump() {
        exitingSlope = true;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        // Claude added cartoonsh snap
        rb.AddForce(moveDir.normalized * (totalSpeed * 0.4f), ForceMode.Impulse);
    }

    private void ResetJump() {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope() {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection() {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    // Stun stuff
    private bool FallingFast() {
        return rb.linearVelocity.y < -maxVelocity;
    }

    public void StunPlayer() {
        // Make sure GetUp isnt running
        CancelInvoke(nameof(GetUp));

        // Set stun eyes
        animator.SetStunEyes();

        // Begin ragdoll process and timer
        ragdoll.ragdollActive = true;
        Invoke(nameof(GetUp), stunTimer);
        Vector3 ragdollForce = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if(debug)
            Debug.Log("Ragdoll applied force: " + ragdollForce);
        ragdoll.EnableRagdoll(ragdollForce / rb.mass);
    }

    private void GetUp() {
        animator.SetNormalEyes();
        ragdoll.EnableAnimator();
    }

    // Setters used to ensure the stats are accurate to boosts
    private void SetMoveSpeed() { totalSpeed = moveSpeed + boostSpeed; }
    private void SetSprintSpeed() { totalSpeed = moveSpeed * sprintSpeedMult + boostSpeed; }
    private void SetCrouchSpeed() { totalSpeed = moveSpeed * crouchSpeedMult + boostSpeed; }
    private void SetStaminaDrain() { totalStaminaDrain = staminaDrain + boostStaminaDrain; }

    // Boosters
    public void BoostSpeed(float boost) { boostSpeed += boost; }
    public void BoostStaminaDrain(float boost) { boostStaminaDrain += boost; }
}
