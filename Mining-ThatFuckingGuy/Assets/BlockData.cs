using System;
using UnityEngine;

[Serializable]
public class BlockData
{
    public int ZIndex;
    public int YIndex;
    public bool IsEmpty;
    public Vector3 WorldPosition;
    public BlockData(int z, int y, bool isEmpty,Vector3 worldPos)
    {
        ZIndex = z;
        YIndex = y;
        IsEmpty = isEmpty;
        WorldPosition = worldPos;
    }
    public void DebugSelf()
    {
        Debug.Log("Z Index: " + ZIndex + " Y Index: " + YIndex + " Is Free: " + IsEmpty);
    }
}
