using UnityEngine;

public class PlayerController : PlayerInfo {
  /*============== Control Info ==============*/
  public RigidBody rb;
  
  public Transform ground_check;
  
  /*============= Main Functions =============*/
  void Start(){
    rb = getComponent<Rigidbody>();
  }

  void Update(){
    if(!getIsStunned() && !getIsDead()){
      playerMovement();
    }

    /*============= Always Running =============*/
    checkGrounded();
  }
  
  /*========== Controller Functions ==========*/
  void playerMovement(){
    player_speed = getSpeed();

    // x and z movement
    x_move = Input.getAxisRaw("Horizontal");
    y_move = Input.getAxisRaw("Vertical");

    // jumping
    if(Input.GetButtonDown("Space")){
      if(getIsGrounded()){
        // apply the jump
      }
    }
  }

  void checkGrounded(){
    // Raycast for check
    // set the is_grounded in PlayerInfo
  }
}
