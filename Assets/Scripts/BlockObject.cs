using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockObject : MonoBehaviour
{
    public int hp;
    
    public void TakeDamage(int damage)
    {
        hp -= damage;
        
        if(hp <= 0)
        {
            Destroy(gameObject);
        }
    }

    void OnBecameVisible()
    {
       // gameObject.GetComponent<Renderer>().enabled = true;
    }

    void OnBecameInvisible()
    {
        //gameObject.GetComponent<Renderer>().enabled = false;
    }
}
