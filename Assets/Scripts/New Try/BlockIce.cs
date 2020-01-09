using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockIce : BlockFire
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Icicle arrow")
        { 
            Destroy(gameObject);
           
        }
        Destroy(other.gameObject);
    }
}
