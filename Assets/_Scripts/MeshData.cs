using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public struct MeshData
{
    public List<Vector3> vertices;
    public List<int> triangles;
    public List<Vector2> uv;

    public List<Vector3> colliderVertices;
    public List<int> colliderTriangles;

    public static MeshData Create()
    {
        return new MeshData()
        {
            vertices = new List<Vector3>(),
            triangles = new List<int>(),
            uv = new List<Vector2>(),
            colliderTriangles = new List<int>(),
            colliderVertices = new List<Vector3>()
        };
    }
    
    public void GetFaceDataIn(Direction direction,int x, int y, int z, BlockType blockType)
    {
        GetFaceVertices(direction, x, y, z,blockType);
        AddQuadTriangles(BlockDataManager.blockTextureDataDictionary[blockType].generatesCollider);
        uv.AddRange(FaceUVs(direction, blockType));
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
    
    private Vector2[] FaceUVs(Direction direction, BlockType blockType)
    {
        Vector2[] UVs = new Vector2[4];
        var tilePos = TexturePosition(direction, blockType);

        UVs[0] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.tileSizeX - BlockDataManager.textureOffset,
            BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.textureOffset);

        UVs[1] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.tileSizeX - BlockDataManager.textureOffset,
            BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.tileSizeY - BlockDataManager.textureOffset);

        UVs[2] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.textureOffset,
            BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.tileSizeY - BlockDataManager.textureOffset);

        UVs[3] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.textureOffset,
            BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.textureOffset);

        return UVs;
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
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);

        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);

        if (quadGeneratesCollider)
        {
            colliderTriangles.Add(colliderVertices.Count - 4);
            colliderTriangles.Add(colliderVertices.Count - 3);
            colliderTriangles.Add(colliderVertices.Count - 2);
            colliderTriangles.Add(colliderVertices.Count - 4);
            colliderTriangles.Add(colliderVertices.Count - 2);
            colliderTriangles.Add(colliderVertices.Count - 1);
        }
    }
}
