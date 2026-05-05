using UnityEngine;
using PurrNet;
using Unity.VisualScripting;

public class PlayerMovement : NetworkIdentity
{
    [Header("Movement")]
    public float moveSpeed;
    
    public float groundDrag;

    public float jumpForce;
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
        rb = GetComponent<Rigidbody>();
        readyToJump = true;
    }

    private void Update() {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        // Debug.DrawRay(transform.position, Vector3.down * (playerHeight * 0.5f + 0.2f), Color.red);

        MyInput();
        SpeedControl();

        // drag
        if(grounded) {
            rb.linearDamping = groundDrag;
        } else {
            rb.linearDamping = 0;
        }
    }

    private void FixedUpdate() {
        MovePlayer();
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
            rb.AddForce(moveDir.normalized * moveSpeed * 10f, ForceMode.Force);
        } else {
            rb.AddForce(moveDir.normalized * moveSpeed * 10f * airMult, ForceMode.Force);
        }
    }

    void SpeedControl() {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if(flatVel.magnitude > moveSpeed) {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    void Jump() {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    void ResetJump() {
        readyToJump = true;
    }
}
