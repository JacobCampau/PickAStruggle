using PurrNet;
using System.Globalization;
using Unity.Cinemachine;
using UnityEngine;

public class ThirdPersonCam : NetworkIdentity 
{
    private RagdollLogic ragdoll;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform player;
    [SerializeField] private Transform playerObj;
    [SerializeField] private CinemachineCamera playerCam;

    [SerializeField] private float rotationSpeed;

    [SerializeField] private CinemachineCamera combatCam;
    [SerializeField] private Transform combatLookAt;
    private bool aiming;

    [SerializeField] private CinemachineCamera ragdollCam;

    protected override void OnSpawned() {
        base.OnSpawned();

        enabled = isOwner;
        playerCam.gameObject.SetActive(isOwner);
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ragdoll = player.GetComponent<RagdollLogic>();

        aiming = false;
    }

    private void LateUpdate() {
        // orientation
        Vector3 viewDir = player.position - new Vector3(playerCam.transform.position.x, player.position.y, playerCam.transform.position.z);
        orientation.forward = viewDir.normalized;

        // rotate player if not stunned
        if(!ragdoll.ragdollActive) {
            ragdollCam.Priority = 5;
            if(aiming) {
                combatCam.Priority = 20;
                playerCam.Priority = 10;

                Vector3 dirForCombat = combatLookAt.position - player.position;
                dirForCombat.y = 0;

                orientation.forward = dirForCombat.normalized;
                //playerObj.forward = dirForCombat.normalized;
                playerObj.forward = Vector3.Slerp(playerObj.forward, dirForCombat.normalized, Time.deltaTime * rotationSpeed);
            } else {
                combatCam.Priority = 10;
                playerCam.Priority = 20;

                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");
                Vector3 inputDir = orientation.forward * verticalInput + orientation.right * horizontalInput;

                if(inputDir != Vector3.zero)
                    playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotationSpeed);
            }
        } else {
            combatCam.Priority = 5;
            playerCam.Priority = 5;
            ragdollCam.Priority = 20;
        }

        // switching cams
        aiming = Input.GetMouseButton(1);
    }
}
