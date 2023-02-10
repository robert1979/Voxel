using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Assertions;

public class VWorldRenderer : MonoBehaviour
{
    public VChunkRenderer chunkPrefab;
    static readonly ProfilerMarker myMarker = new ProfilerMarker("MeshBuild");

    private VWorldData worldData;
    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            worldData = new VWorldData();
            worldData.Init();
            StopWatch.Start();
            myMarker.Begin();
            worldData.Generate();
            myMarker.End();
            StopWatch.End("World Generation took ");
        
            StopWatch.Start();
            RenderWorld(worldData);
            StopWatch.End("World Rendering took ");
        }
    }

    private void RenderWorld(VWorldData worldData)
    {
        var keys = worldData.positionData.GetKeyArray(Allocator.Persistent);
        foreach (var pos in keys)
        {
            var cRenderer = CreateChunk(pos);
            var meshJob = new VMeshJob(pos, worldData.blockData, worldData.positionData);
            cRenderer.StartMeshJob(meshJob);
            
            // Debug.Log($"{meshJob.position}  => " +
            //           $"vertices {meshJob.vertices.Length} " +
            //           $"triangles {meshJob.triangles.Length}  " +
            //           $"uvs {meshJob.uv.Length}");
        }
        keys.Dispose();
    }

    private VChunkRenderer CreateChunk(Vector3Int pos)
    {
        var newChunk = Instantiate<VChunkRenderer>(chunkPrefab);
        newChunk.transform.position = pos;
        return newChunk;
    }
    

    private void Test(VWorldData wData)
    {
        var stride = wData.BlockDataStride;
        Debug.Assert(wData.blockData.Length == stride * wData.positionData.GetKeyArray(Allocator.TempJob).Length," Invalid block size ");

        var keys = wData.positionData.GetKeyArray(Allocator.TempJob);
        foreach (var k in keys)
        {
            var blockStartIdx = 0;
            if (wData.positionData.TryGetValue(k, out blockStartIdx))
            {
                Debug.Log(k + "   block startIdx " + blockStartIdx);
                var slice = new NativeSlice<BlockType>(wData.blockData, blockStartIdx,
                    wData.BlockDataStride);
                var blockData = slice.ToArray();
                Debug.Log(string.Join(' ',blockData));
            }
        }
    }

    private void OnDestroy()
    {
        worldData.Dispose();
    }

}


// var array = new NativeArray<int>(10, Allocator.Persistent);
// for (int i = 0; i < 10; i++)
// {
//     array[i] = i;
// }
//
// var slice = new NativeSlice<int>(array, 0, 3);
// for (int i = 0; i < slice.Length; i++)
// {
//     Debug.Log(slice[i]);
// }
// array.Dispose();