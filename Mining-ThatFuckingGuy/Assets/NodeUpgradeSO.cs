using UnityEngine;

public abstract class NodeUpgradeSO : ScriptableObject
{
    public string DisplayName;
    public NodeCost[] Costs;

    public abstract void Apply(ToolController toolController);
}
[System.Serializable]
public struct NodeCost
{
    public DropSO Drop;
    public int Amount;
}