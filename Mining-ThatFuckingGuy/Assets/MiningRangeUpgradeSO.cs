using UnityEngine;
[CreateAssetMenu(menuName = "Skill Tree/Mining/Range Boost")]
public class MiningRangeUpgradeSO : NodeUpgradeSO
{
    public float UpgradeBoost;

    public override void Apply(ToolController toolController)
    {
        MiningTool tool = toolController.GetMiningTool();
        tool.UpgradeSelf(UpgradeType.Range, UpgradeBoost);
    }
}
