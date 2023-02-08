using System.ComponentModel.Design;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile(CompileSynchronously = true)]
public struct ChunkJob :IJob
{
    public struct MeshData
    {
        public NativeList<int3> Vertices { get; set; }
        public NativeList<int> Triangles { get; set; }
    }

    
    [ReadOnly] public NativeArray<Block> Blocks;
    [WriteOnly] public MeshData meshData;
    [ReadOnly] public BlockData blockData;
    public NativeArray<int3> FaceVertices { get; set; }
    public NativeArray<int> FaceIndices { get; set; }
    public int chunkSize;
    private int vCount;
    

    public void Execute()
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    if (Blocks[BlockExtension.GetBlockIndex(new int3(x, y, z))].IsEmpty()) continue;

                    for (int i = 0; i < 6; i++)
                    {
                        var direction = (Direction)i;
                        if (Check(GetPositionDirection(direction,x,y,z)))
                        {
                            CreateFace(direction,new int3(x,y,z));
                        }
                    }
                }
            }
        }
    }
    
    private void CreateFace(Direction direction, int3 pos)
    {
        var vertices = GetFaceVertices(direction, 1, pos);

        meshData.Vertices.AddRange(vertices);
        vertices.Dispose();
        vCount +=4;
        var vStartIdx = vCount - 4;
        
        meshData.Triangles.Add(vStartIdx);    
        meshData.Triangles.Add(vStartIdx+1);    
        meshData.Triangles.Add(vStartIdx+2);    
        meshData.Triangles.Add(vStartIdx);    
        meshData.Triangles.Add(vStartIdx+2);
        meshData.Triangles.Add(vStartIdx + 3);
    }
    
    private bool Check(int3 position)
    {
        if (position.x >= chunkSize || position.z >= chunkSize || position.x < 0 || position.z < 0)
            return true;
        if (position.y >= chunkSize || position.y<0) return false;

        return Blocks[BlockExtension.GetBlockIndex(position)].IsEmpty();
    }
    
    private NativeArray<int3> GetFaceVertices(Direction direction, int scale, int3 position)
    {
        var faceVertices = new NativeArray<int3>(4,Allocator.Temp);
        for (int i = 0; i<4; i++)
        {
            var idx = ((int)direction) * 4 + i;
            var index = FaceIndices[idx];
            faceVertices[i] = FaceVertices[index] * scale + position;
        }
        return faceVertices;
    }
    
    private int3 GetPositionDirection(Direction direction, int x, int y, int z)
    {
        switch (direction)
        {
            case Direction.Forward:
                return new int3(x, y, z + 1);
            case Direction.Right:
                return new int3(x+1, y, z);
            case Direction.Back:
                return new int3(x, y, z - 1);
            case Direction.Left:
                return new int3(x-1, y, z);
            case Direction.Up:
                return new int3(x, y+1, z);
            case Direction.Down:
                return new int3(x, y-1, z);
            default:
                throw new System.ArgumentOutOfRangeException(nameof(direction), direction, null);
        }
    }
}
