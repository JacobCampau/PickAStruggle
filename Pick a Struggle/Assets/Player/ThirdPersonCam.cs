using PurrNet;
using System.Globalization;
using Unity.Cinemachine;
using UnityEngine;

public class ThirdPersonCam : NetworkIdentity 
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public CinemachineCamera playerCam;
    Rigidbody rb;

    public float rotationSpeed;

    public CinemachineCamera combatCam;
    public Transform combatLookAt;
    public bool aiming;

    protected override void OnSpawned() {
        base.OnSpawned();

        enabled = isOwner;
        playerCam.gameObject.SetActive(isOwner);
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = player.gameObject.GetComponent<Rigidbody>();

        aiming = false;
    }

    private void Update() {
        // orientation
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        // rotate player
        if(aiming) {
            combatCam.Priority = 20;
            playerCam.Priority = 10;

            Vector3 dirForCombat = combatLookAt.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
            orientation.forward = dirForCombat.normalized;

            playerObj.forward = dirForCombat.normalized;
        } else {
            combatCam.Priority = 10;
            playerCam.Priority = 20;

            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if(inputDir != Vector3.zero) {
                playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
        }

        // switching cams
        aiming = Input.GetMouseButton(1);
    }
}
