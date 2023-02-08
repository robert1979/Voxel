using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ChunkRenderer : MonoBehaviour
{
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Mesh mesh;
    public bool showGizmo = false;

    public ChunkData ChunkData { get; private set; }

    public bool ModifiedByThePlayer
    {
        get
        {
            return ChunkData.modifiedByThePlayer;
        }
        set
        {
            ChunkData.modifiedByThePlayer = value;
        }
    }

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        mesh = meshFilter.mesh;
    }

    public void InitializeChunk(ChunkData data)
    {
        this.ChunkData = data;
    }

    private void RenderMesh( MeshData[] meshDataArray)
    {
        mesh.Clear();


        mesh.subMeshCount = 2;
        
        var List = new NativeArray<Vector3>(10,Allocator.Temp);
  
        var vList = new List<Vector3Int>();
        var uvList = new List<Vector2>();
        
        for (int i = 0; i < meshDataArray.Length; i++)
        {
            vList.AddRange(meshDataArray[i].vertices.ToArray());
            uvList.AddRange(meshDataArray[i].uv.ToArray());
        }

        mesh.vertices = vList.Select( v=> new Vector3(v.x,v.y,v.z)).ToArray();
        mesh.uv = uvList.ToArray();

        mesh.SetTriangles(meshDataArray[0].triangles.ToArray(), 0);
        var triangleList = new List<int>(meshDataArray[1].triangles.ToArray());
        mesh.SetTriangles(triangleList.Select(val => val + meshDataArray[0].vertices.Length).ToArray(), 1);
        
        meshCollider.sharedMesh = null;
        Mesh collisionMesh = new Mesh();
        collisionMesh.vertices = meshDataArray[0].colliderVertices.ToArray();
        collisionMesh.triangles = meshDataArray[0].colliderTriangles.ToArray();

        meshDataArray[0].Dispose();
        meshDataArray[1].Dispose();
        mesh.RecalculateNormals();
        collisionMesh.RecalculateNormals();
        meshCollider.sharedMesh = collisionMesh;
    }

    public void UpdateChunk()
    {
        RenderMesh(Chunk.GetChunkMeshData(ChunkData));
    }

    public void UpdateChunk(MeshData[] data)
    {
        RenderMesh(data);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (showGizmo)
        {
            if (Application.isPlaying && ChunkData != null)
            {
                if (Selection.activeObject == gameObject)
                    Gizmos.color = new Color(0, 1, 0, 0.4f);
                else
                    Gizmos.color = new Color(1, 0, 1, 0.4f);

                Gizmos.DrawCube(transform.position + new Vector3(ChunkData.chunkSize / 2f, ChunkData.chunkHeight / 2f, ChunkData.chunkSize / 2f), new Vector3(ChunkData.chunkSize, ChunkData.chunkHeight, ChunkData.chunkSize));
            }
        }
    }
#endif
}
