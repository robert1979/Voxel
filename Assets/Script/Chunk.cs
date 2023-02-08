
using System;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    private MeshFilter meshFilter;
    public int chunkSize =16;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
    }

    private void Start() 
    {
        //Creating Height Map
    }

    private void Update()
    {
        var position = transform.position;
        var blocks = new NativeArray<Block>(chunkSize*chunkSize*chunkSize,Allocator.TempJob);
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                var y = Mathf.FloorToInt(Mathf.PerlinNoise((position.x + x) * 0.15f,
                    (position.z + z) * 0.15f) * chunkSize);

                for (int i = 0; i < y; i++)
                {
                    blocks[BlockExtension.GetBlockIndex(new int3(x, i, z))] = Block.Stone;
                }
                
                for (int i = y; i < chunkSize; i++)
                {
                    blocks[BlockExtension.GetBlockIndex(new int3(x, i, z))] = Block.Air;
                }
            }
        }

        //Schedule Job
        var meshData = new ChunkJob.MeshData()
        {
            Vertices = new NativeList<int3>(Allocator.TempJob),
            Triangles = new NativeList<int>(Allocator.TempJob),
        };

        var jobHandle = new ChunkJob()
        {
            meshData = meshData,
            Blocks = blocks,
            FaceVertices = BlockData.FaceVertices,
            FaceIndices = BlockData.FaceIndices,
            chunkSize = this.chunkSize
        }.Schedule();
        jobHandle.Complete();
        
        //update mesh
        var mesh = new Mesh()
        { 
            vertices = meshData.Vertices.ToArray().Select(vertex => new Vector3(vertex.x,vertex.y,vertex.z)).ToArray(),
            triangles = meshData.Triangles.ToArray()
        };
        
        meshData.Vertices.Dispose();
        meshData.Triangles.Dispose();
        blocks.Dispose();
            
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshFilter.mesh = mesh;
    }

    private void OnDestroy()
    {
        Debug.Log("Destroy -  Disposing");
        BlockData.FaceVertices.Dispose();
        BlockData.FaceIndices.Dispose();
    }
}
