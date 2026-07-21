using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlockData
{
    public int ZIndex;
    public int YIndex;
    public bool IsEmpty;
    public Vector3 WorldPosition;
    public int[] CornerIndex;
    public List<DropReference> DropsOnBlock = new List<DropReference>();
    public BlockData(int z, int y, bool isEmpty, Vector3 worldPos, DungeonManager manager)
    {
        CornerIndex = new int[4];
        ZIndex = z;
        YIndex = y;
        IsEmpty = isEmpty;
        WorldPosition = worldPos;
    }
    public void DebugSelf()
    {
        Debug.Log("Z Index: " + ZIndex + " Y Index: " + YIndex + " Is Free: " + IsEmpty);
    }
    public void CalculateCorners(DungeonManager manager)
    {
        CornerIndex[0] = manager.GetEmptyBlockFromWorldPosition(this, 1, 0) ? -1 : 1;
        CornerIndex[1] = manager.GetEmptyBlockFromWorldPosition(this, 0, -1) ? -1 : 1;
        CornerIndex[2] = manager.GetEmptyBlockFromWorldPosition(this, -1, 0) ? -1 : 1;
        CornerIndex[3] = manager.GetEmptyBlockFromWorldPosition(this, 0, 1) ? -1 : 1;
    }
}
public struct DropReference
{
    public DropType DropType;
    public int DropIndex;
    public DropReference(DropType type, int index)
    {
        DropType = type;
        DropIndex = index;
    }
}
