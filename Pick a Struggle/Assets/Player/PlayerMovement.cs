using UnityEngine;
using PurrNet;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;

public class PlayerMovement : NetworkIdentity
{
    public PlayerStats stats;

    // Move stats
    private float moveSpeed;
    [SerializeField] private float moveSpeedMult = 1.25f;
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

    [Header("Ground Check")]
    [SerializeField] private float playerHeight;
    [SerializeField] private LayerMask whatIsGround;
    private bool grounded;

    [Header("Slope Handling")]
    [SerializeField] private float maxSlopeAngle = 45f;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Other")]
    [SerializeField] private Transform orientation;

    private float horizontalInput;
    private float verticalInput;

    private Vector3 moveDir;

    private Rigidbody rb;

    public enum MovementState { 
        walking, sprinting, air
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
        readyToJump = true;
    }

    private void Update() {
        // Always running, no matter frames
        StateHandler();

        // Ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        if(debug)
            if(grounded) {
                Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f), Color.green);
            } else { 
                Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f), Color.red);
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
    }

    private void FixedUpdate() {
        // Movement
        MovePlayer();

        // Slope Control
    }

    Vector3 GetMovementDir() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        return orientation.forward * verticalInput + orientation.right * horizontalInput;
    }

    void StateHandler() {
        if(grounded && Input.GetKey(sprintKey)) {
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

    void MovePlayer() {
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

        SpeedControl();

        // Cancel gravity on slope
        rb.useGravity = !OnSlope();
    }

    void SpeedControl() {
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

    void Jump() {
        exitingSlope = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        // Claude added cartoonsh snap
        rb.AddForce(moveDir.normalized * (totalSpeed * 0.4f), ForceMode.Impulse);
    }

    void ResetJump() {
        readyToJump = true;
        exitingSlope = false;
    }

    bool OnSlope() {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    Vector3 GetSlopeMoveDirection() {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }

    // Setters used to ensure the stats are accurate to boosts
    void SetMoveSpeed() { totalSpeed = moveSpeed + boostSpeed; }
    void SetSprintSpeed() { totalSpeed = moveSpeed * moveSpeedMult + boostSpeed; }
    void SetStaminaDrain() { totalStaminaDrain = staminaDrain + boostStaminaDrain; }
}
