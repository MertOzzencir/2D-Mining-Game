using UnityEngine;

public abstract class NodeUpgradeSO : ScriptableObject
{
    public string DisplayName;
    public NodeCost[] Costs;
    public abstract int MaxLevel { get; }
    public abstract void Apply(ToolController toolController, int level);
}
public abstract class StatUpgradeSO : NodeUpgradeSO
{
    public float[] UpgradeBoost;
    public override int MaxLevel => UpgradeBoost.Length;
   
}
[System.Serializable]
public struct NodeCost
{
    public DropSO Drop;
    public int Amount;
}