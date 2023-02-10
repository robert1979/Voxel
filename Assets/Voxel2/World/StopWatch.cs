


using UnityEngine;

public class StopWatch
{
    private static float startTime;

    public static void Start()
    {
        startTime = Time.realtimeSinceStartup;
    }

    public static void End(string message)
    {
        Debug.Log($"{message}  {Mathf.RoundToInt((Time.realtimeSinceStartup-startTime)*1000)}ms");
    }
}
