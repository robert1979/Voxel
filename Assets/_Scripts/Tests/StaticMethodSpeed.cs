using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticMethodSpeed : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var t = Time.realtimeSinceStartup;
        for (int p = 0; p < 10000; p++)
        {
            Process(10);
        }
        
        Debug.Log((Time.realtimeSinceStartup -t)*1000);
        var adder = new Adder();

        t = Time.realtimeSinceStartup;
        for (int p = 0; p < 10000; p++)
        {
            adder.Process(10);
        }
        
        Debug.Log((Time.realtimeSinceStartup -t)*1000);
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static void Process(float a)
    {
        for (int i = 0; i < 100; i++)
        {
            Mathf.Sqrt(a);
        }
    }
    
}

public struct Adder
{
    public void Process(float a)
    {
        for (int i = 0; i < 100; i++)
        {
            Mathf.Sqrt(a);
        }
    }
}
