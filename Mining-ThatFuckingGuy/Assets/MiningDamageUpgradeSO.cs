using UnityEngine;

[CreateAssetMenu(menuName = "Skill Tree/Mining/Damage Boost")]
public class MiningDamageUpgradeSO : NodeUpgradeSO
{
    public float UpgradeBoost;

    public override void Apply(ToolController toolController)
    {
        MiningTool tool = toolController.GetMiningTool();
        tool.UpgradeSelf(UpgradeType.Damage, UpgradeBoost);
    }
}