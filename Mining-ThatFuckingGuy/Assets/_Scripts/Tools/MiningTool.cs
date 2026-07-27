using System.Collections;
using UnityEngine;

public class MiningTool : ToolBase
{
    [SerializeField] private LayerMask destructable;
    [SerializeField] private LayerMask dropLayerMask;
    [SerializeField] private Transform storagedPlacement;
    [SerializeField] private LaserBeam laser;
    private Vector3 direction;
    private float timer;
    private MiningToolSO data => Data as MiningToolSO;

    public override void Awake()
    {
        base.Awake();
        stats[UpgradeType.ToolDamage] = data.Damage;
        stats[UpgradeType.ToolCooldown] = data.CooldownTimer;
        stats[UpgradeType.ToolMaxRange] = data.Range;
        laser.UpdateRange(stats[UpgradeType.ToolMaxRange]);
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
                        d.Destruct(stats[UpgradeType.ToolDamage], out _);
                        timer = 0;
                    }
                }
            }
        }
        else if (AlternativeState)
        {
            CollectInCone();
        }
    }

    private void CollectInCone()
    {
        DungeonManager currentManager = PlayerController.CurrentDungeon;
        Plane plane = new Plane(Vector3.right, currentManager.transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            BlockData currentData = currentManager.GetBlockFromWorldPosition(hitPoint, out _);
            foreach (var a in currentData.DropsOnBlock)
            {
                Vector3 dropPos = currentManager.instancedDropRenderer.GetDropPosition(a.DropType, a.DropIndex);
                currentManager.instancedDropRenderer.RemoveDrop(a.DropType, a.DropIndex);

                Transform proxy = currentManager.CheckoutDropProxy(a.DropType, dropPos);
                float duration = Mathf.Clamp(Vector3.Distance(transform.position, dropPos), 0f, data.CollectAnimationTimer);
                StartCoroutine(CollectAnimation(a.DropType, currentManager, proxy, dropPos, duration));
            }
            currentData.DropsOnBlock.Clear();
        }
    }

    private IEnumerator CollectAnimation(DropType type, DungeonManager manager, Transform drop, Vector3 startPosition, float animationDuration)
    {
        float duration = animationDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 refStorage = storagedPlacement.position;
            refStorage.x = 0;
            Vector3 center = Vector3.Lerp(startPosition, refStorage, 0.5f) - Vector3.up * 0.3f;
            center.x = 0;
            Vector3 startRelative = startPosition - center;
            Vector3 endRelative = refStorage - center;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 arced = Vector3.Slerp(startRelative, endRelative, t);
            drop.position = center + arced;

            yield return null;
        }

        manager.ReturnDropProxy(type, drop);
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void UpgradeSelf(UpgradeData upgradeData)
    {
        stats[upgradeData.Type] += upgradeData.Amount;
        laser.UpdateRange(stats[UpgradeType.ToolMaxRange]);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(AimPositionTransform.position, AimPositionTransform.forward * stats[UpgradeType.ToolMaxRange]);
    }
}