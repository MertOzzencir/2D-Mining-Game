using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class MiningTool : ToolBase
{
    [SerializeField] private LayerMask destructable;
    [SerializeField] private LayerMask dropLayerMask;
    [SerializeField] private Transform storagedPlacement;
    private Vector3 direction;
    private float timer;
    private DropBase[] storagedDrops;
    private List<DropBase> listOfAnimatedDrops = new List<DropBase>();
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
        Collider[] hits = Physics.OverlapSphere(AimPositionTransform.position, stats[UpgradeType.ToolMaxRange], dropLayerMask);
        List<DropBase> currentCollected = new List<DropBase>();
        foreach (Collider col in hits)
        {
            if (!col.TryGetComponent(out DropBase d) || d.IsCollected) continue;

            Vector3 toTarget = (col.transform.position - AimPositionTransform.position).normalized;
            float angle = Vector3.Angle(direction, toTarget);

            if (angle <= data.ConeAngle)
            {
                for (int i = 0; i < storagedDrops.Length; i++)
                {
                    if (storagedDrops[i] == null)
                    {
                        d.Collect();
                        currentCollected.Add(d);
                        listOfAnimatedDrops.Add(d);
                        d.IndexInStorage = i;
                        storagedDrops[i] = d;
                        break;
                    }
                }
            }
        }
        if (currentCollected.Count > 0)
        {
            StartCoroutine(ControlAnimationTimer(currentCollected, .01f));
        }
    }
    private IEnumerator ControlAnimationTimer(List<DropBase> lists, float waitTimer)
    {
        float timer = data.CollectAnimationTimer;
        foreach (var a in lists)
        {
            a.AnimationLogic();
            StartCoroutine(CollectAnimation(a, a.transform.position, timer));
            yield return new WaitForSeconds(waitTimer);
        }
    }
    private IEnumerator CollectAnimation(DropBase drop, Vector3 startPosition, float animationDuration)
    {

        float duration = animationDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Vector3 refStorage = storagedPlacement.position;
            refStorage.x = 0;
            Vector3 center = Vector3.Lerp(startPosition, refStorage, 0.5f) - Vector3.up * 0.3f; // biraz aşağıda bir merkez
            center.x = 0;
            Vector3 startRelative = startPosition - center;
            Vector3 endRelative = refStorage - center;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 arced = Vector3.Slerp(startRelative, endRelative, t);
            drop.transform.position = center + arced;

            yield return null;
        }

        drop.transform.parent = storagedPlacement;
        drop.gameObject.SetActive(false);
        drop.transform.localPosition = Vector3.zero;
        drop.transform.localEulerAngles = Vector3.zero;
        listOfAnimatedDrops.Remove(drop);
    }
    public override void OnDisable()
    {
        base.OnDisable();
        if (listOfAnimatedDrops.Count > 0)
        {
            foreach (var a in listOfAnimatedDrops)
            {
                a.UnCollect();
                storagedDrops[a.IndexInStorage] = null;
                a.IndexInStorage = 0;
            }
        }
        listOfAnimatedDrops.Clear();
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