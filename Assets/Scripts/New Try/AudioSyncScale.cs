using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSyncScale : AudioSynce
{
    public GameObject[] cubes;
    public Transform[] points;

    private void SpawnBlock()
    {
        if (m_isBeat) { 
            GameObject cube = Instantiate(cubes[Random.Range(0, 2)], points[Random.Range(0, 4)]);
            cube.transform.localPosition = Vector3.zero;
            cube.transform.Rotate(transform.forward, 90 * Random.Range(0, 4));
        }
    }
   

    public override void OnBeat()
    {
        base.OnBeat();
        
        
        StartCoroutine("SpawnBlock");
        StopCoroutine("SpawnBlock");
    }
  
}
