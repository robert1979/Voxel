using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockHelper
{
    public static Vector3[] FaceVertices = new[]
    {
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1),
        new Vector3(0, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(0, 1, 0),
        new Vector3(1, 1, 0),
        new Vector3(1, 0, 0),
        new Vector3(0, 0, 0),
    };

    public static int[][] FaceIndices = new[]
    {
        new int[]{3,0, 1, 2},
        new int[]{6,5, 0, 3},
        new int[]{7,4, 5, 6},
        new int[]{2,1, 4, 7},
        new int[]{0,5, 4, 1}, 
        new int[]{6,3, 2, 7}
    };
    
    private static Direction[] directions =
    {
        Direction.backwards,
        Direction.down,
        Direction.forward,
        Direction.left,
        Direction.right,
        Direction.up
    };

    public static MeshData GetMeshData
        (ChunkData chunk, int x, int y, int z, MeshData meshData,MeshData waterMeshData, BlockType blockType)
    {
        if (blockType == BlockType.Air || blockType == BlockType.Nothing)
            return meshData;

        foreach (Direction direction in directions)
        {
            var neighbourBlockCoordinates = new Vector3Int(x, y, z) + direction.GetVector();
            var neighbourBlockType = Chunk.GetBlockFromChunkCoordinates(chunk, neighbourBlockCoordinates);

            if (neighbourBlockType != BlockType.Nothing && BlockDataManager.blockTextureDataDictionary[neighbourBlockType].isSolid == false)
            {
                if (blockType == BlockType.Water)
                {
                    if (neighbourBlockType == BlockType.Air)
                        waterMeshData.GetFaceDataIn(direction, x, y, z, blockType);
                }
                else
                {
                    meshData.GetFaceDataIn(direction, x, y, z, blockType);
                }
            }
        }
        return meshData;
    }
}
