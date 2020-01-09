using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
   
    public GameObject Icicle;
    public GameObject Fireball;
    private Vector3 mousepos;
    void Start()
    {
        Cursor.visible=true;
    }
    // Update is called once per frame
    void Update()
    {
        Soot();
    }

    private void Soot()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            mousepos = Input.mousePosition;
            mousepos.z = 3;

            mousepos = Camera.main.ScreenToWorldPoint(mousepos);
            Instantiate(Icicle, mousepos, Quaternion.identity);
        }
        if (Input.GetMouseButtonDown(1))
        {
            mousepos = Input.mousePosition;
            mousepos.z = 3;

            mousepos = Camera.main.ScreenToWorldPoint(mousepos);
            Instantiate(Fireball, mousepos, Quaternion.identity);
        }

    }

}
