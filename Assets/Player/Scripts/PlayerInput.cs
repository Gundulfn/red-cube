using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerInput : NetworkBehaviour
{
    private PlayerMovement playerMovement;
    private Inventory inventory;
    public CamRaycast camRaycast;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        inventory = GetComponent<Inventory>();
        camRaycast = GameObject.Find("Camera").GetComponent<CamRaycast>();
    }

    void Update()
    {
        if (!this.isLocalPlayer)
            return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        bool run = Input.GetButton("Run");

        Vector3 movement = transform.right * x + transform.forward * z;

        playerMovement.Move(movement * playerMovement.currentSpeed * Time.deltaTime, run);

        bool jump = Input.GetButtonDown("Jump");

        if (jump)
        {
            playerMovement.Jump();
        }

        if (camRaycast.isHit)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 pos = camRaycast.GetHitFacePos();
                Item currentItem = inventory.GetCurrentItem();

                if (currentItem.GetType() == typeof(Block))
                {
                    GetComponent<ChunkManager>().SendChunkUpdateRequestMessage(pos, currentItem.id);       
                }

            }
            else if (Input.GetMouseButtonDown(1))
            {
                Vector3 pos = camRaycast.GetHitObject().transform.position;
                
                GetComponent<ChunkManager>().SendChunkUpdateRequestMessage(pos);
                
            }
        }

        // Toolbelt
        // If any key is pressed, then check if it's an number key and set active item
        if(Input.anyKey)
        {
            for (int i = 0; i < 10; ++i)
            {
                if (Input.GetKeyDown("" + i))
                {
                    if (i != 0)
                    { 
                        inventory.SetActiveItem(i - 1); 
                    }
                    else
                    { 
                        inventory.SetActiveItem(9); 
                    }
                }
            }
        }
    }
}