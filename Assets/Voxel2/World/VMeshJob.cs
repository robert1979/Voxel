using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;

public struct Byte3
{
    public readonly byte x;
    public readonly byte y;
    public readonly byte z;

    public Byte3(byte x, byte y, byte z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
}

[BurstCompile]
public struct VMeshJob : IJob
{
    public readonly Vector3Int position;
    public readonly int BlockDataStartIdx;
    public readonly int BlockDataEndIdx;
    private readonly int ChunkBlockDataLength;
    public readonly NativeArray<Byte3> FaceVertices;
    public readonly NativeArray<Direction> Directions;
    public readonly NativeArray<Vector3Int> VectorDirections;
    
    
    [ReadOnly] NativeList<BlockType> worldblockData;
    [ReadOnly] NativeHashMap<Vector3Int,int> worldpositionData;

    public NativeList<Vector3Int> vertices;
    public NativeList<int> triangles;
    public NativeList<Vector2> uv;

    public NativeList<Vector3> colliderVertices;
    public NativeList<int> colliderTriangles;
    
    private static int chunkSize => WorldGenerationSettings.chunkSize;
    private static int chunkHeight => WorldGenerationSettings.chunkHeight;
    private static int mapSizeInChunks => WorldGenerationSettings.mapSizeInChunks;
    public readonly NativeArray<Byte> FaceIndices;
    
    public VMeshJob(Vector3Int position,
        NativeList<BlockType> worldblockData,
        NativeHashMap<Vector3Int, int> worldpositionData)
    {
        this.position = position;
        this.worldblockData = worldblockData;
        this.worldpositionData = worldpositionData;
        vertices = new NativeList<Vector3Int>(Allocator.Persistent);
        triangles = new NativeList<int>(Allocator.Persistent);
        uv = new NativeList<Vector2>(Allocator.Persistent);
        colliderVertices = new NativeList<Vector3>(Allocator.Persistent);
        colliderTriangles = new NativeList<int>(Allocator.Persistent);
        ChunkBlockDataLength = chunkSize * chunkSize * chunkHeight;
        
        if (!worldpositionData.TryGetValue(position, out BlockDataStartIdx))
        {
            throw new Exception("Cannot retrieve block data for chunk " + position);
        }
        BlockDataEndIdx = BlockDataStartIdx + ChunkBlockDataLength-1;
        
        FaceVertices = new  NativeArray<Byte3>(8,Allocator.Persistent)
        {
            [0] = new Byte3(1, 1, 1),
            [1] = new Byte3(0, 1, 1),
            [2] = new Byte3(0, 0, 1),
            [3] = new Byte3(1, 0, 1),
            [4] = new Byte3(0, 1, 0),
            [5] = new Byte3(1, 1, 0),
            [6] = new Byte3(1, 0, 0),
            [7] = new Byte3(0, 0, 0),
        };
        
        Directions = new  NativeArray<Direction>(6,Allocator.Persistent)
        {
            [0] = Direction.backwards,
            [1] = Direction.down,
            [2] = Direction.forward,
            [3] = Direction.left,
            [4] = Direction.right,
            [5] = Direction.up
        };
        
        VectorDirections = new NativeArray<Vector3Int>(6,Allocator.Persistent)
        {
            [0] = Vector3Int.forward,
            [1] = Vector3Int.right,
            [2] = Vector3Int.back,
            [3] = Vector3Int.left,
            [4] = Vector3Int.up,
            [5] = Vector3Int.down
        };

        FaceIndices = new NativeArray<Byte>(new Byte[]
        {
            3, 0, 1, 2,
            6, 5, 0, 3,
            7, 4, 5, 6,
            2, 1, 4, 7,
            0, 5, 4, 1,
            6, 3, 2, 7
        }, Allocator.Persistent);
    }
    
    public void Dispose()
    {
        vertices.Dispose();
        triangles.Dispose();
        uv.Dispose();
        colliderVertices.Dispose();
        colliderTriangles.Dispose();
        FaceVertices.Dispose();
        Directions.Dispose();
        VectorDirections.Dispose();
        FaceIndices.Dispose();
    }
    
    public void Execute()
    {
        CreateChunkMesh();
    }

    public void CreateChunkMesh()
    {
        for (int index = 0; index < ChunkBlockDataLength; index++)
        {
            var pos = IndexToLocalPos(index);
            var blockType = worldblockData[BlockDataStartIdx+ LocalPosToIndex(pos.x, pos.y, pos.z)];
            CreateBlockMesh(pos.x, pos.y, pos.z, blockType);
        }
    }
    
    public Vector3Int GetVector(Direction direction)
    {
        return VectorDirections[(int)direction];
    }
    
    private void CreateBlockMesh(int x, int y, int z, BlockType blockType)
    {
        if (blockType == BlockType.Air || blockType == BlockType.Nothing) return;
        
        foreach (Direction direction in Directions)
        {
            var neighbourBlockCoordinates = new Vector3Int(x, y, z) + GetVector(direction);
            var neighbourBlockType = GetBlockFromLocalPos(BlockDataStartIdx, neighbourBlockCoordinates);
        
            //if (neighbourBlockType != BlockType.Nothing && BlockDataManager.lookUpList[(int)neighbourBlockType].isSolid == false)
            if (!IsSolid(neighbourBlockType))
            {
                GetFaceDataIn(direction, x, y, z, blockType);
            }
        }
    }

    public BlockType GetBlockFromLocalPos(int chunkDataStartIdx, Vector3Int position)
        => GetBlockFromLocalPos(chunkDataStartIdx, position.x, position.y, position.z);

    
    public BlockType GetBlockFromLocalPos(int chunkDataStartIdx,int x, int y, int z)
    {
        if (InRange(x) && InRangeHeight(y) && InRange(z))
        {
            return worldblockData[chunkDataStartIdx + LocalPosToIndex(x, y, z)];
        }
        return GetBlockFromWorldPos(position.x + x, position.y + y, position.z + z);
    }

    public BlockType GetBlockFromWorldPos(int x, int y, int z)
    {
        Vector3Int neighbourChunkWorldPos = ChunkToBlockCoords(x, y, z);
        int chunkDataStartIdx =0;
        if (worldpositionData.TryGetValue(neighbourChunkWorldPos, out chunkDataStartIdx))
        {
            Vector3Int blockInChunkCoordinates = BlockToChunkCoordinates(neighbourChunkWorldPos, new Vector3Int(x, y, z));
            
            return GetBlockFromLocalPos(chunkDataStartIdx, 
                blockInChunkCoordinates.x,blockInChunkCoordinates.y,blockInChunkCoordinates.z);
        }
        else
        {
            return BlockType.Nothing;
        }
    }
    
    public static Vector3Int BlockToChunkCoordinates(Vector3Int neighborWorldPosition, Vector3Int worldPos) => new Vector3Int
        {
            x = worldPos.x - neighborWorldPosition.x,
            y = worldPos.y - neighborWorldPosition.y,
            z = worldPos.z - neighborWorldPosition.z
        };

    public static Vector3Int ChunkToBlockCoords(int x, int y, int z) => new Vector3Int
        {
            x = (int)math.floor(x / (float)chunkSize) * chunkSize,
            y = (int)math.floor(y / (float)chunkHeight) * chunkHeight,
            z = (int)math.floor(z / (float)chunkSize) * chunkSize
        };
        
    public void GetFaceDataIn(Direction direction,int x, int y, int z, BlockType blockType)
    {
        GetFaceVertices(direction, x, y, z,blockType);
        //AddQuadTriangles(BlockDataManager.lookUpList[(int)blockType].generatesCollider);
        AddQuadTriangles(IsSolid(blockType));

        //AddFaceUVs(direction, blockType);
    }
    
    private void GetFaceVertices(Direction direction, int x, int y, int z, BlockType blockType)
    {
        var generatesCollider = IsSolid(blockType);// BlockDataManager.lookUpList[(int)blockType].generatesCollider;
        
        for (int i = 0; i < 4; i++)
        {
            var faceVertexIdx = FaceIndices[((int)direction*4)+i];
            var fVertices = FaceVertices[faceVertexIdx];
            var v = new Vector3Int(x + fVertices.x,y + fVertices.y,z + fVertices.z);
            vertices.Add(v);
            if (generatesCollider)
            {
                colliderVertices.Add(v);
            }
        }
    }

    private bool IsSolid(BlockType blockType)
    {
        return blockType != BlockType.Air && blockType != BlockType.Water && blockType != BlockType.Nothing;
    }
    
    // private void AddFaceUVs(Direction direction, BlockType blockType)
    // {
    //     var UVs = new NativeArray<Vector2>(4,Allocator.Temp);
    //     var tilePos = TexturePosition(direction, blockType);
    //
    //     UVs[0] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.tileSizeX - BlockDataManager.textureOffset,
    //         BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.textureOffset);
    //
    //     UVs[1] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.tileSizeX - BlockDataManager.textureOffset,
    //         BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.tileSizeY - BlockDataManager.textureOffset);
    //
    //     UVs[2] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.textureOffset,
    //         BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.tileSizeY - BlockDataManager.textureOffset);
    //
    //     UVs[3] = new Vector2(BlockDataManager.tileSizeX * tilePos.x + BlockDataManager.textureOffset,
    //         BlockDataManager.tileSizeY * tilePos.y + BlockDataManager.textureOffset);
    //     uv.AddRange(UVs);
    //     UVs.Dispose();
    // }
    
    // private Vector2Int TexturePosition(Direction direction, BlockType blockType)
    // {
    //     return direction switch
    //     {
    //         Direction.up => BlockDataManager.lookUpList[(int)blockType].up,
    //         Direction.down => BlockDataManager.lookUpList[(int)blockType].down,
    //         _ => BlockDataManager.lookUpList[(int)blockType].side
    //     };
    // }

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

    public static bool InRange(int axisCoordinate) => !(axisCoordinate < 0 || axisCoordinate >= chunkSize);
    //in chunk coordinate system
    public static bool InRangeHeight(int ycoordinate) => !(ycoordinate < 0 || ycoordinate >= chunkHeight);
    
    public static int LocalPosToIndex(int x, int y, int z) => x + chunkSize * y + chunkSize * chunkHeight * z;
    
    public static Vector3Int IndexToLocalPos(int index)
    {
        int x = index % chunkSize;
        int y = (index / chunkSize) % chunkHeight;
        int z = index / (chunkSize * chunkHeight);
        return new Vector3Int(x, y, z);
    }

 
}