using Unity.Burst;
using Unity.Collections;
using UnityEngine;

//[BurstCompile]
public struct VWorldData
{
    private static int chunkSize => WorldGenerationSettings.chunkSize;
    private static int chunkHeight => WorldGenerationSettings.chunkHeight;
    
    private static int mapSizeInChunks => WorldGenerationSettings.mapSizeInChunks;
    private static int waterThreshold => WorldGenerationSettings.waterThreshold;
    private static float noiseScale => WorldGenerationSettings.noiseScale;

    public NativeList<BlockType> blockData;
    public NativeHashMap<Vector3Int,int> positionData;
    
    public void Init()
    {
        blockData = new NativeList<BlockType>(Allocator.Persistent);
        positionData = new NativeHashMap<Vector3Int,int>(100,Allocator.Persistent);
    }

    public int BlockDataStride => WorldGenerationSettings.chunkSize * WorldGenerationSettings.chunkSize * WorldGenerationSettings.chunkHeight;

    public void Dispose()
    {
        blockData.Dispose();
        positionData.Dispose();
    }

    public void Generate()
    {
        for (int x = 0; x < mapSizeInChunks; x++)
        {
            for (int z = 0; z < mapSizeInChunks; z++)
            {
                var pos = new Vector3Int(x * chunkSize, 0, z * chunkSize);
                GenerateVoxels(pos);
            }
        }
    }
    
    private void GenerateVoxels(Vector3Int worldPosition)
    {
        positionData.Add(worldPosition,blockData.Length);
        var chunkData = new NativeArray<BlockType>(chunkSize * chunkSize * chunkHeight, Allocator.TempJob);;
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                float noiseValue = Mathf.PerlinNoise((worldPosition.x + x) * noiseScale, (worldPosition.z + z) * noiseScale);
                int groundPosition = Mathf.RoundToInt(noiseValue * chunkHeight);
                for (int y = 0; y < chunkHeight; y++)
                {
                    BlockType voxelType = BlockType.Dirt;
                    if (y > groundPosition)
                    {
                        // if (y < waterThreshold)
                        // {
                        //     voxelType = BlockType.Water;
                        // }
                        // else
                        {
                            voxelType = BlockType.Air;
                        }

                    }
                    else if (y == groundPosition && y < waterThreshold)
                    {
                        voxelType = BlockType.Sand;
                    }
                    else if (y == groundPosition)
                    {
                        voxelType = BlockType.Grass_Dirt;
                    }
                    SetBlock(chunkData,new Vector3Int(x, y, z), voxelType);
                }
            }
        }
        blockData.AddRange(chunkData);
        chunkData.Dispose();
    }
    
    public void SetBlock(NativeArray<BlockType> chunkData, Vector3Int localPosition, BlockType block)
    {
        if (InRange(localPosition.x) && InRangeHeight(localPosition.y) && InRange(localPosition.z))
        {
            int index = GetIndexFromPosition(localPosition.x, localPosition.y, localPosition.z);
            chunkData[index] = block;
        }
        else
        {
            throw new System.Exception("Need to ask World for appropiate chunk");
        }
    }
    
    private int GetIndexFromPosition( int x, int y, int z)
    {
        return x + chunkSize * y + chunkSize * chunkHeight * z;
    }
    
    //in chunk coordinate system
    private bool InRangeHeight(int ycoordinate) => !(ycoordinate < 0 || ycoordinate >= chunkHeight);

    private bool InRange(int axisCoordinate) => !(axisCoordinate < 0 || axisCoordinate >= chunkSize);

}