using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BlockDataManager : MonoBehaviour
{
    public static float textureOffset = 0.001f;
    public static float tileSizeX, tileSizeY;
    public BlockDataSO textureData;
    public static TextureData[] lookUpList;

    private void Awake()
    {
        tileSizeX = textureData.textureSizeX;
        tileSizeY = textureData.textureSizeY;

        var bTypeValues = Enum.GetValues(typeof(BlockType));
        lookUpList = new TextureData[bTypeValues.Length];
        for (var i = 0; i < lookUpList.Length; i++)
        {
            var type = (BlockType)i;
            var tData = textureData.textureDataList.Find(a=>a.blockType == type);
            lookUpList[i] = tData ?? null;
        }
    }
}
