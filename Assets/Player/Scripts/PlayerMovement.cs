using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
    public CharacterController characterController;
    public Stamina stamina;
    public Transform groundChecker;
    public LayerMask groundMask;
    private GameObject _camera;

    public bool isRunning;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float currentSpeed;

    public float gravity = -9.81f;
    public float jumpHeight = 1f;

    private Vector3 velocity;
    private float groundDistance = 0.4f;
    private bool isGrounded = true;

    private int jumpStaminaCost = 0;
    
    void Start()
    {
        if (this.isLocalPlayer)
        {            
            _camera = GameObject.Find("Camera");
            _camera.transform.parent = gameObject.transform;
            _camera.transform.position = new Vector3(0, 0.8f, 0.6f) + gameObject.transform.position;
            _camera.GetComponent<MouseLook>().enabled = true;
            _camera.GetComponent<MouseLook>().SetPlayerTransform(transform);
            
            currentSpeed = walkSpeed;
        }
    }

    public void Move(Vector3 movement, bool run)
    {
        if(run && stamina.canDoAction() 
            && movement != Vector3.zero) // to detect if player's pressing movement keys
        {
            isRunning = true;
            currentSpeed = runSpeed;
        }
        else
        {
            isRunning = false;
            currentSpeed = walkSpeed;
        }

        characterController.Move(movement);
    }

    public void Jump() {
        if (isGrounded && stamina.canDoAction(jumpStaminaCost))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            stamina.ReduceStamina( jumpStaminaCost );
        }
    }    

    void Update()
    {
        if (this.isLocalPlayer)
        {
            // Check if grounded
            isGrounded = Physics.CheckSphere(groundChecker.position, groundDistance, groundMask);

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }

    // when game is closed, save player
    void OnApplicationQuit()
    {
        if(this.isServer)
        {
            // SaveSystem.SavePlayer(this);
        }
        else
        {
            //ClientMessage.SendSaveMsg(this);
        }
    }
}