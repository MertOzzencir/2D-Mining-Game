using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlockData
{
    public DestructableBase CurrentBlock;
    public GameObject BlackDust;
    public GameObject wall;
    public int ZIndex;
    public int YIndex;
    public bool IsEmpty;
    public Vector3 WorldPosition;
    public Vector3 ColliderSize;
    public int[] CornerIndex;
    public List<DropReference> DropsOnBlock = new List<DropReference>();
    public BlockData(int z, int y, bool isEmpty, Vector3 worldPos, Vector3 colliderSize)
    {
        CornerIndex = new int[4];
        ZIndex = z;
        YIndex = y;
        IsEmpty = isEmpty;
        WorldPosition = worldPos;
        ColliderSize = colliderSize;
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
    public Vector3 GetPoint(BlockPoint point)
    {
        float hx = ColliderSize.x / 2f;
        float hy = ColliderSize.y / 2f;
        float hz = ColliderSize.z / 2f;

        switch (point)
        {
            case BlockPoint.FrontTopLeft: return new Vector3(hx, hy, -hz);
            case BlockPoint.FrontTopRight: return new Vector3(hx, hy, hz);
            case BlockPoint.FrontBottomLeft: return new Vector3(hx, -hy, -hz);
            case BlockPoint.FrontBottomRight: return new Vector3(hx, -hy, hz);

            case BlockPoint.BackTopLeft: return new Vector3(-hx, hy, -hz);
            case BlockPoint.BackTopRight: return new Vector3(-hx, hy, hz);
            case BlockPoint.BackBottomLeft: return new Vector3(-hx, -hy, -hz);
            case BlockPoint.BackBottomRight: return new Vector3(-hx, -hy, hz);

            case BlockPoint.FrontTop: return new Vector3(hx, hy, 0);
            case BlockPoint.FrontBottom: return new Vector3(hx, -hy, 0);
            case BlockPoint.FrontLeft: return new Vector3(hx, 0, -hz);
            case BlockPoint.FrontRight: return new Vector3(hx, 0, hz);

            case BlockPoint.BackTop: return new Vector3(-hx, hy, 0);
            case BlockPoint.BackBottom: return new Vector3(-hx, -hy, 0);
            case BlockPoint.BackLeft: return new Vector3(-hx, 0, -hz);
            case BlockPoint.BackRight: return new Vector3(-hx, 0, hz);

            default: return Vector3.zero;
        }
    }

    public Vector3 GetWorldPoint(BlockPoint point)
    {
        return WorldPosition + GetPoint(point);
    }
    public BlockPoint GetReverse(BlockPoint point)
    {
        switch (point)
        {
            case BlockPoint.FrontTopLeft: return BlockPoint.BackTopLeft;
            case BlockPoint.FrontTopRight: return BlockPoint.BackTopRight;
            case BlockPoint.FrontBottomLeft: return BlockPoint.BackBottomLeft;
            case BlockPoint.FrontBottomRight: return BlockPoint.BackBottomRight;

            case BlockPoint.BackTopLeft: return BlockPoint.FrontTopLeft;
            case BlockPoint.BackTopRight: return BlockPoint.FrontTopRight;
            case BlockPoint.BackBottomLeft: return BlockPoint.FrontBottomLeft;
            case BlockPoint.BackBottomRight: return BlockPoint.FrontBottomRight;

            case BlockPoint.FrontTop: return BlockPoint.BackTop;
            case BlockPoint.FrontBottom: return BlockPoint.BackBottom;
            case BlockPoint.FrontLeft: return BlockPoint.BackLeft;
            case BlockPoint.FrontRight: return BlockPoint.BackRight;

            case BlockPoint.BackTop: return BlockPoint.FrontTop;
            case BlockPoint.BackBottom: return BlockPoint.FrontBottom;
            case BlockPoint.BackLeft: return BlockPoint.FrontLeft;
            case BlockPoint.BackRight: return BlockPoint.FrontRight;

            default: return point;
        }
    }
}
public enum BlockPoint
{
    FrontTopLeft, FrontTopRight, FrontBottomLeft, FrontBottomRight,
    BackTopLeft, BackTopRight, BackBottomLeft, BackBottomRight,
    FrontTop, FrontBottom, FrontLeft, FrontRight,
    BackTop, BackBottom, BackLeft, BackRight
}
public struct DropReference
{
    public DropSO Data;
    public int DropIndex;
    public DropReference(DropSO data, int index)
    {
        Data = data;
        DropIndex = index;
    }
}