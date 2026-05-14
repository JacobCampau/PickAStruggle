using UnityEngine;

public class PlayerAnimatior : MonoBehaviour
{
    private Animator anim;
    private PlayerMovement player;
    private RagdollLogic ragdoll;
    

    [SerializeField] private GameObject normalEyes;
    [SerializeField] private GameObject deadEyes;
    [SerializeField] private GameObject stunnedEyes;

    private void Update() {
        anim = GetComponent<Animator>();
        player = GetComponent<PlayerMovement>();
        ragdoll = GetComponent<RagdollLogic>(); 
    }

    private void Update(){
        if(ragdoll.ragdollIsActive){
        
        }
    }

    public void SetStunEyes() {
        stunnedEyes.SetActive(true);
        normalEyes.SetActive(false);
        deadEyes.SetActive(false);
    }

    public void SetNormalEyes() {
        stunnedEyes.SetActive(false);
        normalEyes.SetActive(true);
        deadEyes.SetActive(false);
    }
}
