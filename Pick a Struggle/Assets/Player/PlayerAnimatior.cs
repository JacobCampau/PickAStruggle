using PurrNet;
using UnityEngine;

public class PlayerAnimatior : NetworkIdentity
{
    private Animator anim;
    private PlayerMovement player;
    private RagdollLogic ragdoll;
    private PlayerCombat combat;

    [SerializeField] private bool debug;

    [Header("Objects")]
    [SerializeField] private GameObject normalEyes;
    [SerializeField] private GameObject deadEyes;
    [SerializeField] private GameObject stunnedEyes;

    [Header("Time on ground")]
    [SerializeField] private float stunTimer;

    private Rigidbody rb;

    private void Start() {
        anim = GetComponentInChildren<Animator>();
        player = GetComponent<PlayerMovement>();
        ragdoll = GetComponent<RagdollLogic>(); 
        combat = GetComponent<PlayerCombat>();

        rb = GetComponent<Rigidbody>();
    }

    private void Update(){
        if(ragdoll.ragdollActive){
            // Player is not moving
            if(combat.isDead){
                SetDeadEyes();
            }else{
                SetStunEyes();
            }
        }else{
            // Player is moving
            SetNormalEyes();

            // Lower body animation triggers and bools
            if(player.state == PlayerMovement.MovementState.walking) {
                
            }else if(player.state == PlayerMovement.MovementState.sprinting){
                
            }else if(player.state == PlayerMovement.MovementState.crouch){
                
            }else if(player.state == PlayerMovement.MovementState.air){
                
            }
            
            // Upper body animation triggers and bools
        }
    }

    // Ragdoll stunning
    public void StunPlayer(bool getBackUp, Vector3 force, float mult) {
        // Make sure GetUp isnt running
        CancelInvoke(nameof(GetUp));

        // Begin ragdoll process and timer
        if(getBackUp)
            Invoke(nameof(GetUp), stunTimer);
        
        if(debug)
            Debug.Log("Ragdoll applied force: " + force);
        ragdoll.EnableRagdoll(mult * force / rb.mass);
    }

    private void GetUp() {
        ragdoll.EnableAnimator();
    }

    private void SetNormalEyes() {
        normalEyes.SetActive(true);
        deadEyes.SetActive(false);
        stunnedEyes.SetActive(false);
    }

    private void SetDeadEyes() {
        normalEyes.SetActive(false);
        deadEyes.SetActive(true);
        stunnedEyes.SetActive(false);
    }

    private void SetStunEyes() {
        normalEyes.SetActive(false);
        deadEyes.SetActive(false);
        stunnedEyes.SetActive(true);
    }
}
