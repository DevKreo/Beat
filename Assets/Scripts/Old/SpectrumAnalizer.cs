using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectrumAnalizer : MonoBehaviour
{
    AudioSource source;
    GameObject[] cubes;

    public float SpectrumRefreshTime;
    private float lastUpdate = 0;
    private float[] spectrum = new float[1024];
    public float scaleFactor = 10000;
    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
        cubes = new GameObject[1024];
        createDisplayObjects();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - lastUpdate > SpectrumRefreshTime)
        {
            source.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
            for (int i = 0; i < spectrum.Length; i++)
            {
                cubes[i].transform.position= new Vector3(1, spectrum[i], 1);
            }
            lastUpdate = Time.time;
        }
    }
    void createDisplayObjects()
    {
        for (int i = 0; i < 1024; i++)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = new Vector3(i, 0, 0);
            cubes[i] = cube;
        }
    }
}
