using UnityEngine;
[CreateAssetMenu(menuName = "Skill Tree/Mining/Cooldown Boost")]
public class MiningCooldownUpgradeSO : StatUpgradeSO
{

    public override void Apply(ToolController toolController, int level)
    {
        MiningTool tool = toolController.GetMiningTool();
        tool.UpgradeSelf(UpgradeType.Cooldown, UpgradeBoost[level]);
    }
}
