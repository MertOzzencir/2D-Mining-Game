using System.Collections.Generic;
using UnityEngine;

public class MiningTool : ToolBase
{
    [SerializeField] private LayerMask destructable;
    [SerializeField] private Transform visual;
    [SerializeField] private Transform aimPosition;
    [SerializeField] private Transform storagedPlacement;
    private Vector3 direction;
    private float timer;
    private DropBase[] storagedDrops;
    private MiningToolSO data => Data as MiningToolSO;

    public void Awake()
    {
        stats[UpgradeType.ToolDamage] = data.Damage;
        stats[UpgradeType.ToolCooldown] = data.CooldownTimer;
        stats[UpgradeType.ToolMaxRange] = data.Range;

        storagedDrops = new DropBase[data.StorageLimit];
    }
    private void HandleRotation(Transform t)
    {
        Plane plane = new Plane(Vector3.right, t.transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            //hitPoint.x = 0;
            Vector3 direction = (hitPoint - t.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            t.rotation = lookRotation;
        }
    }
    public override void UpdateUse()
    {
        HandleRotation(visual);
        HandleRotation(aimPosition);

        timer += Time.deltaTime;
        direction = aimPosition.forward;
        direction.x = 0;
        direction = direction.normalized;
        Ray hitRay = new Ray(aimPosition.position, direction);
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
        Gizmos.DrawRay(aimPosition.position, aimPosition.forward * stats[UpgradeType.ToolMaxRange]);
    }

}
