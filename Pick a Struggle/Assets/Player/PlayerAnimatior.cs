using UnityEngine;

public class PlayerAnimatior : NetworkIdentity
{
    private Animator anim;
    private PlayerMovement player;
    private RagdollLogic ragdoll;
    private PlayerCombat combat;
    
    [SerializeField] private GameObject normalEyes;
    [SerializeField] private GameObject deadEyes;
    [SerializeField] private GameObject stunnedEyes;

    private void Start() {
        anim = GetComponent<Animator>();
        player = GetComponent<PlayerMovement>();
        ragdoll = GetComponent<RagdollLogic>(); 
        combat = GetComponent<PlayerCombat>();
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

            // Lower body animations
            if(player.MovementState == walking){
                
            }else if(player.MovementState == sprinting){
                
            }else if(player.MovementState == crouch){
                
            }else if(player.MovementState == air){
                
            }
            
            // Upper body animations
        }
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
