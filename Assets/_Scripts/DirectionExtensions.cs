using System;
using UnityEngine;

public static class DirectionExtensions
{
    public static Vector3Int[] VectorDirections = new Vector3Int[]
    {
        Vector3Int.forward,
        Vector3Int.right,
        Vector3Int.back,
        Vector3Int.left,
        Vector3Int.up,
        Vector3Int.down
    };
    
    public static Vector3Int GetVector(this Direction direction)
    {
        return VectorDirections[(int)direction];
    }
}
