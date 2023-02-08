using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;

public struct MeshData
{
    public NativeList<Vector3> vertices;
    public NativeList<int> triangles;
    public NativeList<Vector2> uv;

    public NativeList<Vector3> colliderVertices;
    public NativeList<int> colliderTriangles;

    public static MeshData Create()
    {
        return new MeshData()
        {
            vertices = new NativeList<Vector3>(Allocator.TempJob),
            triangles = new NativeList<int>(Allocator.TempJob),
            uv = new NativeList<Vector2>(Allocator.TempJob),
            colliderTriangles = new NativeList<int>(Allocator.TempJob),
            colliderVertices = new NativeList<Vector3>(Allocator.TempJob)
        };
    }

    public void Dispose()
    {
        vertices.Dispose();
        triangles.Dispose();
        uv.Dispose();
        colliderVertices.Dispose();
        colliderTriangles.Dispose();
    }
    
    public void GetFaceDataIn(Direction direction,int x, int y, int z, BlockType blockType)
    {
        GetFaceVertices(direction, x, y, z,blockType);
        AddQuadTriangles(BlockDataManager.blockTextureDataDictionary[blockType].generatesCollider);
        AddFaceUVs(direction, blockType);
    }
    
    private void GetFaceVertices(Direction direction, int x, int y, int z, BlockType blockType)
    {
        var generatesCollider = BlockDataManager.blockTextureDataDictionary[blockType].generatesCollider;
        
        var faceVertexIndices = BlockHelper.FaceIndices[(int)direction];
        for (int i = 0; i < 4; i++)
        {
            var v = BlockHelper.FaceVertices[faceVertexIndices[i]] + new Vector3(x,y,z);
            vertices.Add(v);
            if (generatesCollider)
            {
                colliderVertices.Add(v);
            }
        }
    }
    
    private void AddFaceUVs(Direction direction, BlockType blockType)
    {
        var UVs = new NativeArray<Vector2>(4,Allocator.Temp);
        var tilePos = TexturePosition(direction, blockType);

        UVs[0] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.tileSizeX - BlockDataManager.textureOffset,
            BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.textureOffset);

        UVs[1] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.tileSizeX - BlockDataManager.textureOffset,
            BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.tileSizeY - BlockDataManager.textureOffset);

        UVs[2] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.textureOffset,
            BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.tileSizeY - BlockDataManager.textureOffset);

        UVs[3] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.textureOffset,
            BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.textureOffset);
        uv.AddRange(UVs);
        UVs.Dispose();
    }
    
    private Vector2Int TexturePosition(Direction direction, BlockType blockType)
    {
        return direction switch
        {
            Direction.up => BlockDataManager.blockTextureDataDictionary[blockType].up,
            Direction.down => BlockDataManager.blockTextureDataDictionary[blockType].down,
            _ => BlockDataManager.blockTextureDataDictionary[blockType].side
        };
    }

    private void AddQuadTriangles(bool quadGeneratesCollider)
    {
        var vCount = vertices.Length;
        triangles.Add(vCount - 4);
        triangles.Add(vCount - 3);
        triangles.Add(vCount - 2);

        triangles.Add(vCount - 4);
        triangles.Add(vCount - 2);
        triangles.Add(vCount - 1);

        var cCount = colliderVertices.Length;
        if (quadGeneratesCollider)
        {
            colliderTriangles.Add(cCount - 4);
            colliderTriangles.Add(cCount - 3);
            colliderTriangles.Add(cCount - 2);
            colliderTriangles.Add(cCount - 4);
            colliderTriangles.Add(cCount - 2);
            colliderTriangles.Add(cCount - 1);
        }
    }
}
