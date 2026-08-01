using UnityEngine;
[CreateAssetMenu(menuName = "Skill Tree/Mining/Cooldown Boost")]
public class MiningCooldownUpgradeSO : NodeUpgradeSO
{
    public float UpgradeBoost;

    public override void Apply(ToolController toolController)
    {
        MiningTool tool = toolController.GetMiningTool();
        tool.UpgradeSelf(UpgradeType.Cooldown, UpgradeBoost);
    }
}
