using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public enum Block : ushort
{
     Null,
     Air,
     GrassDirt,
     Dirt,
     Grass_Stone,
     Stone,
     TreeTrunk,
     TreeLeavesTransparent,
     TreeLeavesSolid,
     Water,
     Sand
}

public enum Direction
{
     Forward,
     Right,
     Back,
     Left,
     Up,
     Down
}

public struct BlockData
{
     [ReadOnly]
     public static readonly NativeArray<int3> FaceVertices = new NativeArray<int3>(8,Allocator.Persistent)
     {
        [0] =  new int3(1, 1, 1),
        [1] = new int3(0, 1, 1),
        [2] = new int3(0, 0, 1),
        [3] = new int3(1, 0, 1),
        [4] = new int3(0, 1, 0),
        [5] = new int3(1, 1, 0),
        [6] = new int3(1, 0, 0),
        [7] = new int3(0, 0, 0),
     };
     
     [ReadOnly]
     public static readonly NativeArray<int> FaceIndices = new NativeArray<int>(24,Allocator.Persistent)
     {
          [0] = 0, [1] = 1, [2] = 2, [3] = 3,
          [4] = 5, [5] = 0, [6] = 3, [7] = 6,
          [8] = 4, [9] = 5, [10] = 6, [11] = 7,
          [12] = 1, [13]= 4, [14] = 7, [15] = 2,
          [16] = 5, [17] = 4, [18] = 1, [19] = 0, 
          [20] = 3, [21] = 2, [22] = 7, [23] = 6
     };
}

public static class BlockExtension
{
     public static int GetBlockIndex(int3 position)=>position.x + position.z * 16 + position.y * 16 *16;

     public static bool IsEmpty(this Block block) => block == Block.Air;

}