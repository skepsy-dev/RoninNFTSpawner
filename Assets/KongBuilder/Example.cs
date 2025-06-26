using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 pos = Vector3.zero;
        KongBuilder.Instance.LoadKong(810, pos, true ,true);//This example will generate fingers
        pos += new Vector3(3, 0, 0);
        KongBuilder.Instance.LoadKong(810, pos, false, true);//This example won't generate fingers
        pos += new Vector3(3, 0, 0);

        //This example generates 10 random kongz with fingers
        for (int i = 1; i < 10; i++)
        {
            KongBuilder.Instance.LoadKong(Random.Range(1, 15000), pos, true);
            pos += new Vector3(3, 0, 0);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
