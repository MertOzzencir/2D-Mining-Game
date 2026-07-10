using System.Collections.Generic;
using UnityEngine;

public class MiningTool : ToolBase
{
    [SerializeField] private LayerMask destructable;

    [SerializeField] private Transform storagedPlacement;
    private Vector3 direction;
    private float timer;
    private DropBase[] storagedDrops;
    private MiningToolSO data => Data as MiningToolSO;

    public override void Awake()
    {
        base.Awake();
        stats[UpgradeType.ToolDamage] = data.Damage;
        stats[UpgradeType.ToolCooldown] = data.CooldownTimer;
        stats[UpgradeType.ToolMaxRange] = data.Range;

        storagedDrops = new DropBase[data.StorageLimit];
    }

    public override void UpdateUse()
    {
        base.UpdateUse();


        timer += Time.deltaTime;
        direction = AimPositionTransform.forward;
        direction.x = 0;
        direction = direction.normalized;
        Ray hitRay = new Ray(AimPositionTransform.position, direction);
        if (MainUseState)
        {
            if (Physics.Raycast(hitRay, out RaycastHit hit, stats[UpgradeType.ToolMaxRange], destructable))
            {
                if (hit.transform.TryGetComponent(out DestructableBase d))
                {
                    if (timer > stats[UpgradeType.ToolCooldown])
                    {
                        d.Destruct(stats[UpgradeType.ToolDamage]);
                        timer = 0;
                    }
                }
            }
        }
        else
        {
            if (AlternativeState)
            {
                if (Physics.Raycast(hitRay, out RaycastHit hit, stats[UpgradeType.ToolMaxRange]))
                {
                    if (hit.transform.TryGetComponent(out DropBase d))
                    {
                        for (int i = 0; i < storagedDrops.Length; i++)
                        {
                            if (storagedDrops[i] == null)
                            {
                                if (!d.IsCollected)
                                {
                                    d.Collect(storagedPlacement, data.CollectAnimationTimer);
                                    storagedDrops[i] = d;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

    }
    public override void UpgradeSelf(UpgradeData upgradeData)
    {
        stats[upgradeData.Type] += upgradeData.Amount;
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawRay(AimPositionTransform.position, AimPositionTransform.forward * stats[UpgradeType.ToolMaxRange]);
    }

}
