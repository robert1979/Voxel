using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class PointerTest : MonoBehaviour
{

    public NativeMultiHashMap<Vector3, BlockType> world;
    // Start is called before the first frame update
    void Start()
    {
        world = new NativeMultiHashMap<Vector3, BlockType>(5,Allocator.Persistent);
        for (int i = 0; i < 7; i++)
        {
            world.Add(Vector3.one, (BlockType)i);
        }

        Debug.Log(world.Count());
        var t = "";
        foreach (var val in world.GetValuesForKey(Vector3.one))
        {
            t += val + " ";
        }

        Debug.Log(t);

        // var w = new WorldJob() { world = world };
        // Debug.Log(  w.world[Vector3.zero]);
        //
        // w.Schedule().Complete();
        //
        // world[Vector3.zero] = 88;
        // Debug.Log(  w.world[Vector3.zero]);
        world.Dispose();
    }
}

