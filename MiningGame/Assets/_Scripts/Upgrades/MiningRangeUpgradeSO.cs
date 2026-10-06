using UnityEngine;
[CreateAssetMenu(menuName = "Skill Tree/Mining/Range Boost")]
public class MiningRangeUpgradeSO : StatUpgradeSO
{


    public override void Apply(ToolController toolController, int level)
    {
        MiningTool tool = toolController.GetMiningTool();
        tool.UpgradeSelf(UpgradeType.Range, UpgradeBoost[level]);
    }
}