using UnityEngine;
using PurrNet;

public class PlayerMovement : NetworkIdentity
{
    public PlayerStats stats;

    // Move stats
    float moveSpeed; 
    float staminaMax = 100;
    float staminaDrain = 5;
    float currStamina;
    float jumpForce;

    float boostSpeed;
    float boostStaminaDrain;

    float totalSpeed;
    float totalStaminaDrain;

    // Movement stats (same for all characters)
    [Header("Debug")]
    public bool debug;

    [Header("Movement")]
    public float groundDrag;
    public float jumpCooldown;
    public float airMult;
    bool readyToJump;

    [Header("Key Binds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDir;

    Rigidbody rb;

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
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        if(debug)
            if(grounded) {
                Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f), Color.green);
            } else { 
                Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f), Color.red);
            }

        MyInput();

        // drag
        if(grounded) {
            rb.linearDamping = groundDrag;
        } else {
            rb.linearDamping = 0;
        }
    }

    private void FixedUpdate() {
        MovePlayer();
        SpeedControl();
    }

    void MyInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // jump
        if(Input.GetKey(jumpKey) && grounded && readyToJump) {
            readyToJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    void MovePlayer() {
        // calc move direction
        moveDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if(grounded) {
            rb.AddForce(moveDir.normalized * totalSpeed * 10f, ForceMode.Force);
        } else {
            rb.AddForce(moveDir.normalized * totalSpeed * 10f * airMult, ForceMode.Force);
        }
    }

    void SpeedControl() {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if(flatVel.magnitude > totalSpeed) {
            Vector3 limitedVel = flatVel.normalized * totalSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    void Jump() {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        // Claude added cartoonsh snap
        rb.AddForce(moveDir.normalized * (totalSpeed * 0.4f), ForceMode.Impulse);
    }

    void ResetJump() {
        readyToJump = true;
    }

    // Setters used to ensure the stats are accurate to boosts
    void SetMoveSpeed() { totalSpeed = moveSpeed + boostSpeed; }
    void SetStaminaDrain() { totalStaminaDrain = staminaDrain + boostStaminaDrain; }
}
