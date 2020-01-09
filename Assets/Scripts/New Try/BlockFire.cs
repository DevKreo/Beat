using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockFire : MonoBehaviour
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

    protected void Move()
    {
        transform.position += Time.deltaTime * transform.forward * 10;
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Fireball")
        { 
            Destroy(gameObject);
            
        }
        Destroy(other.gameObject);
    }

}
