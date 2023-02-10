using Unity.Collections;
using UnityEngine;

public class MeshDataJob
{
    public NativeArray<BlockType> blocks;
    public NativeList<Vector3Int> vertices;
    public NativeList<int> triangles;
    public NativeList<Vector2> uv;

    public NativeList<Vector3> colliderVertices;
    public NativeList<int> colliderTriangles;

    [ReadOnly]
    public static Vector3Int[] FaceVertices = new[]
    {
        new Vector3Int(1, 1, 1),
        new Vector3Int(0, 1, 1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(1, 0, 1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(0, 0, 0),
    };
    
    [ReadOnly]
    public static int[][] FaceIndices = new[]
    {
        new int[]{3,0, 1, 2},
        new int[]{6,5, 0, 3},
        new int[]{7,4, 5, 6},
        new int[]{2,1, 4, 7},
        new int[]{0,5, 4, 1}, 
        new int[]{6,3, 2, 7}
    };
    
    [ReadOnly]
    private static Direction[] directions =
    {
        Direction.backwards,
        Direction.down,
        Direction.forward,
        Direction.left,
        Direction.right,
        Direction.up
    };
    
    public MeshDataJob(BlockType[] blocks)
    {
        this.blocks = new NativeArray<BlockType>(blocks, Allocator.TempJob);
        triangles = new NativeList<int>(Allocator.TempJob);
        uv = new NativeList<Vector2>(Allocator.TempJob);
        colliderTriangles = new NativeList<int>(Allocator.TempJob);
        colliderTriangles = new NativeList<int>(Allocator.TempJob);
    }
    
    public void GetMeshData
        (ChunkData chunk, int x, int y, int z, MeshData meshData,MeshData waterMeshData, BlockType blockType)
    {
        if (blockType == BlockType.Air || blockType == BlockType.Nothing)
            return;

        foreach (Direction direction in directions)
        {
            var neighbourBlockCoordinates = new Vector3Int(x, y, z) + direction.GetVector();
            var neighbourBlockType = Chunk.GetBlockFromChunkCoordinates(chunk, neighbourBlockCoordinates);

            if (neighbourBlockType != BlockType.Nothing && BlockDataManager.lookUpList[(int)neighbourBlockType].isSolid == false)
            {
                if (blockType == BlockType.Water)
                {
                    if (neighbourBlockType == BlockType.Air)
                        GetFaceDataIn(direction, x, y, z, blockType);
                }
                else
                {
                    GetFaceDataIn(direction, x, y, z, blockType);
                }
            }
        }
    }
    
    
    public void GetFaceDataIn(Direction direction,int x, int y, int z, BlockType blockType)
    {
        GetFaceVertices(direction, x, y, z,blockType);
        AddQuadTriangles(BlockDataManager.lookUpList[(int)blockType].generatesCollider);
        AddFaceUVs(direction, blockType);
    }
    
    private void GetFaceVertices(Direction direction, int x, int y, int z, BlockType blockType)
    {
        var generatesCollider = BlockDataManager.lookUpList[(int)blockType].generatesCollider;
        
        var faceVertexIndices = BlockHelper.FaceIndices[(int)direction];
        for (int i = 0; i < 4; i++)
        {
            var v = BlockHelper.FaceVertices[faceVertexIndices[i]] + new Vector3Int(x,y,z);
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
            Direction.up => BlockDataManager.lookUpList[(int)blockType].up,
            Direction.down => BlockDataManager.lookUpList[(int)blockType].down,
            _ => BlockDataManager.lookUpList[(int)blockType].side
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
    
    public void Dispose()
    {
        blocks.Dispose();
        vertices.Dispose();
        triangles.Dispose();
        uv.Dispose();
        colliderVertices.Dispose();
        colliderTriangles.Dispose();
    }
    
    
}
