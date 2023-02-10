using System;
using UnityEngine;

public static class VBlockFunctions
{
    private static int chunkSize => WorldGenerationSettings.chunkSize;
    private static int chunkHeight => WorldGenerationSettings.chunkHeight;
    
    
    private static Vector3Int GetPostitionFromIndex(ChunkData chunkData, int index)
    {
        int x = index % chunkSize;
        int y = (index / chunkSize) % chunkHeight;
        int z = index / (chunkSize * chunkHeight);
        return new Vector3Int(x, y, z);
    }

    //in chunk coordinate system
    private static bool InRange(ChunkData chunkData, int axisCoordinate)
    {
        if (axisCoordinate < 0 || axisCoordinate >= chunkSize)
            return false;

        return true;
    }

    //in chunk coordinate system
    private static bool InRangeHeight(ChunkData chunkData, int ycoordinate)
    {
        if (ycoordinate < 0 || ycoordinate >= chunkHeight)
            return false;

        return true;
    }

    public static BlockType GetBlockFromChunkCoordinates(ChunkData chunkData, Vector3Int chunkCoordinates)
    {
        return GetBlockFromChunkCoordinates(chunkData, chunkCoordinates.x, chunkCoordinates.y, chunkCoordinates.z);
    }

    public static BlockType GetBlockFromChunkCoordinates(ChunkData chunkData, int x, int y, int z)
    {
        if (InRange(chunkData, x) && InRangeHeight(chunkData, y) && InRange(chunkData, z))
        {
            int index = GetIndexFromPosition(chunkData, x, y, z);
            return chunkData.blocks[index];
        }

        return chunkData.worldReference.GetBlockFromChunkCoordinates(chunkData, chunkData.worldPosition.x + x, chunkData.worldPosition.y + y, chunkData.worldPosition.z + z);
    }

    public static void SetBlock(ChunkData chunkData, Vector3Int localPosition, BlockType block)
    {
        if (InRange(chunkData, localPosition.x) && InRangeHeight(chunkData, localPosition.y) && InRange(chunkData, localPosition.z))
        {
            int index = GetIndexFromPosition(chunkData, localPosition.x, localPosition.y, localPosition.z);
            chunkData.blocks[index] = block;
        }
        else
        {
            throw new Exception("Need to ask World for appropiate chunk");
        }
    }

    private static int GetIndexFromPosition(ChunkData chunkData, int x, int y, int z)
    {
        return x + chunkSize * y + chunkSize * chunkHeight * z;
    }

    public static Vector3Int GetBlockInChunkCoordinates(ChunkData chunkData, Vector3Int worldPos)
    {
        return new Vector3Int
        {
            x = worldPos.x - chunkData.worldPosition.x,
            y = worldPos.y - chunkData.worldPosition.y,
            z = worldPos.z - chunkData.worldPosition.z
        };
    }

    public static MeshData[] GetChunkMeshData(ChunkData chunkData)
    {
        MeshData meshData = MeshData.Create();
        MeshData waterMeshData = MeshData.Create();

        for (int index = 0; index < chunkData.blocks.Length; index++)
        {
            var pos = GetPostitionFromIndex(chunkData, index);
            meshData = BlockHelper.GetMeshData(chunkData,
                pos.x, pos.y, pos.z,
                meshData,waterMeshData,
                chunkData.blocks[GetIndexFromPosition(chunkData, pos.x, pos.y, pos.z)]);
        }

        return  new MeshData[]{meshData, waterMeshData};
    }

    internal static Vector3Int ChunkPositionFromBlockCoords(World world, int x, int y, int z)
    {
        Vector3Int pos = new Vector3Int
        {
            x = Mathf.FloorToInt(x / (float)chunkSize) * chunkSize,
            y = Mathf.FloorToInt(y / (float)chunkHeight) * chunkHeight,
            z = Mathf.FloorToInt(z / (float)chunkSize) * chunkSize
        };
        return pos;
    }
}