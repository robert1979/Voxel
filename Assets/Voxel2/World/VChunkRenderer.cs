using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class VChunkRenderer : MonoBehaviour
{
    private MeshFilter mFilter;
    private MeshCollider mCollider;
    private Mesh mesh;
    private JobHandle jobHandle;
    private VMeshJob meshJob;
    private bool hasStarted;

    private void Awake()
    {
        mFilter = GetComponent<MeshFilter>();
        mCollider = GetComponent<MeshCollider>();
        mesh = mFilter.mesh;
 
    }

    public void StartMeshJob(VMeshJob meshJob)
    {
        this.meshJob = meshJob;
        jobHandle = meshJob.Schedule();
        hasStarted = true;
    }

    private void LateUpdate()
    {
        if (!hasStarted)
        {
            return;
        }
        

        if (jobHandle.IsCompleted)
        {
            //Debug.Log("Finished " + meshJob.position);
            hasStarted = true;
            jobHandle.Complete();
            hasStarted = false;
            RenderMesh(meshJob);
        }
    }

    private void OnDestroy()
    {
        meshJob.Dispose();
    }

    private void RenderMesh(VMeshJob meshJob)
    {
        mesh.Clear();
        mesh.subMeshCount = 1;
        
        var vList = new List<Vector3Int>();
        vList.AddRange(meshJob.vertices.ToArray());
        mesh.vertices = vList.Select( v=> new Vector3(v.x,v.y,v.z)).ToArray();
        mesh.uv = meshJob.uv.ToArray();
        
        mesh.SetTriangles(meshJob.triangles.ToArray(), 0);
        
        mCollider.sharedMesh = null;
        Mesh collisionMesh = new Mesh();
        collisionMesh.vertices = meshJob.colliderVertices.ToArray();
        collisionMesh.triangles = meshJob.colliderTriangles.ToArray();
        mesh.RecalculateNormals();
        collisionMesh.RecalculateNormals();
        mCollider.sharedMesh = collisionMesh;
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawCube(transform.position, Vector3.one);
    }
    
}
