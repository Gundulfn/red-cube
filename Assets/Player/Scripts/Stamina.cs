using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stamina : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private StateText stateText;

    public int initialStamina = 100;
    public int currentStamina;
    
    public float initialRegenTime = 0.1f;
    private float currentRegenTime;

    public float initialReduceTime = 0.5f;
    private float currentReduceTime;

    void Start() {
        playerMovement = GetComponent<PlayerMovement>();
        stateText = GameObject.Find("StaminaText").GetComponent<StateText>();

        currentStamina = initialStamina;
        currentRegenTime = initialRegenTime;
        currentReduceTime = initialReduceTime;
    }

    void Update()
    {
        if(playerMovement.isRunning)
        {
            ReduceStamina();
        }
        else
        {
            RegenStamina();
        }      
    }

    public void ReduceStamina(int cost = 0) 
    {
        if(cost == 0)
        {
            if(currentStamina > 0) 
            {
                currentRegenTime = initialRegenTime; // reset currentRegenTime when reducing
                currentReduceTime -= Time.deltaTime;

                if(currentReduceTime <= 0) 
                {
                    currentStamina--;
                    currentReduceTime = initialReduceTime;
                }
            }
        }
        else
        {
            currentStamina -= cost;
        }

        UpdateStaminaState();
    }

    public void RegenStamina()
    {
        if(currentStamina < initialStamina)
        {
            currentReduceTime = initialReduceTime; // reset currentReduceTime when regening
            currentRegenTime -= Time.deltaTime;

            if(currentRegenTime <= 0)
            {
                currentStamina++;
                currentRegenTime = initialRegenTime;
            } 

            UpdateStaminaState();  
        }
    }

    private void UpdateStaminaState()
    {
        stateText.UpdateStateText("SP: " + currentStamina.ToString()); 
        currentStamina = Mathf.Clamp(currentStamina, 0, initialStamina); 
    } 

    // If action with/without cost is able to do
    public bool canDoAction(int staminaCost = 0)
    {
        if(staminaCost == 0)
        {
            if(currentStamina > 0) 
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
        else
        {
            return currentStamina - staminaCost >= 0;
        }
    }
}