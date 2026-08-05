using UnityEngine;

[CreateAssetMenu(menuName = "Skill Tree/Mining/Damage Boost")]
public class MiningDamageUpgradeSO : StatUpgradeSO
{

    public override void Apply(ToolController toolController, int level)
    {
        MiningTool tool = toolController.GetMiningTool();
        tool.UpgradeSelf(UpgradeType.Damage, UpgradeBoost[level]);
    }
}