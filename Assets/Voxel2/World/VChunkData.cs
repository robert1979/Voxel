

using Unity.Collections;
using UnityEngine;

public struct VChunkData
{
    public Vector3Int position;
    public NativeArray<BlockType> blockData;

    public VChunkData(Vector3Int position, NativeArray<BlockType> blockData)
    {
        this.position = position;
        this.blockData = blockData;
    }
}