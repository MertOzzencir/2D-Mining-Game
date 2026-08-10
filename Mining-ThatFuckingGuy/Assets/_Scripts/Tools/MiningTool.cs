using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class MiningTool : ToolBase
{
    [SerializeField] private LayerMask destructable;
    [SerializeField] private LayerMask dropLayerMask;
    [SerializeField] private Transform storagedPlacement;
    [SerializeField] private LaserBeam laser;
    [SerializeField] private ParticleSystem hitVFX;
    [SerializeField] private ParticleSystem hitNonStopVFX;
    private Vector3 direction;
    private float timer;
    private MiningToolSO data => Data as MiningToolSO;
    private Dictionary<DropSO, int> collectedDropsDict = new Dictionary<DropSO, int>();
    private int currentDropCollectedTotal;
    private ParticleSystemRenderer hitVFXRenderer;
    public override void Awake()
    {
        base.Awake();
        hitVFXRenderer = hitVFX.GetComponent<ParticleSystemRenderer>();
        hitNonStopVFX.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }
    private bool wasHitting = false;

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
            bool isHittingNow = false;

            if (Physics.Raycast(hitRay, out RaycastHit hit, Stats[UpgradeType.Range], destructable))
            {
                if (hit.transform.TryGetComponent(out DestructableBase d))
                {
                    isHittingNow = true;

                    if (!wasHitting)
                    {
                        hitNonStopVFX.Play();
                    }

                    hitNonStopVFX.gameObject.transform.position = hit.point;
                    hitNonStopVFX.transform.forward = hit.normal;

                    if (timer > Stats[UpgradeType.Cooldown])
                    {
                        Material renderer = d.GetMaterial();
                        renderer.SetFloat("_DestroyOffSet", d.CurrentHealthRatio());
                        d.Destruct(Stats[UpgradeType.Damage], out _);
                        timer = 0;
                        hitVFX.gameObject.transform.position = d.transform.position;
                        hitVFXRenderer.material.SetColor("_BaseColor", d.Data.Color);
                        hitVFX.Play();
                    }
                }
            }
            else
            {
                hitVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }

            if (!isHittingNow && wasHitting)
            {
                hitNonStopVFX.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }

            wasHitting = isHittingNow;
            return;
        }
        else if (AlternativeState)
        {
            if (wasHitting)
            {
                hitNonStopVFX.Stop(false, ParticleSystemStopBehavior.StopEmitting);
                wasHitting = false;
            }
            CollectInCone();
            return;
        }

        if (wasHitting)
        {
            hitNonStopVFX.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            wasHitting = false;
        }
        hitVFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void CollectInCone()
    {
        if (currentDropCollectedTotal >= Stats[UpgradeType.StorageLimit]) return;

        DungeonManager currentManager = PlayerController.CurrentDungeon;
        Plane plane = new Plane(Vector3.right, currentManager.transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);
            BlockData currentData = currentManager.GetBlockFromWorldPosition(hitPoint, out _);

            for (int i = currentData.DropsOnBlock.Count - 1; i >= 0; i--)
            {
                DropReference currentDrop = currentData.DropsOnBlock[i];
                Vector3 dropPos = currentManager.instancedDropRenderer.GetDropPosition(currentDrop.Data.DropType, currentDrop.DropIndex);
                currentManager.instancedDropRenderer.RemoveDrop(currentDrop.Data.DropType, currentDrop.DropIndex);

                Transform proxy = currentManager.CheckoutDropProxy(currentDrop.Data.DropType, dropPos);
                float duration = Mathf.Clamp(Vector3.Distance(transform.position, dropPos), 0f, data.CollectAnimationTimer);
                currentDropCollectedTotal++;
                StartCoroutine(CollectAnimation(currentDrop.Data, currentManager, proxy, dropPos, duration));
                currentData.DropsOnBlock.RemoveAt(i);

                if (currentDropCollectedTotal >= Stats[UpgradeType.StorageLimit]) break;
            }
        }
    }

    public override void SetStats()
    {
        base.SetStats();
        Stats[UpgradeType.Damage] = data.Damage;
        Stats[UpgradeType.StorageLimit] = data.StorageLimit;
        laser.UpdateRange(Stats[UpgradeType.Range]);
    }
    private IEnumerator CollectAnimation(DropSO dropData, DungeonManager manager, Transform drop, Vector3 startPosition, float animationDuration)
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

        if (collectedDropsDict.ContainsKey(dropData))
            collectedDropsDict[dropData] += 1;
        else
            collectedDropsDict[dropData] = 1;
        manager.ReturnDropProxy(dropData.DropType, drop);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        hitNonStopVFX.Stop(false, ParticleSystemStopBehavior.StopEmitting);
    }



    public Dictionary<DropSO, int> DropsOnTool()
    {
        return collectedDropsDict;
    }
    public void ResetStorage()
    {
        currentDropCollectedTotal = 0;
        collectedDropsDict.Clear();
    }
    public override void UpgradeSelf(UpgradeType type, float amount)
    {
        base.UpgradeSelf(type, amount);
        switch (type)
        {
            case UpgradeType.Damage:
                Stats[UpgradeType.Damage] += amount;
                break;
        }
        laser.UpdateRange(Stats[UpgradeType.Range]);
    }

    void OnDrawGizmos()
    {
        if (Stats == null) return;
        Gizmos.DrawRay(AimPositionTransform.position, AimPositionTransform.forward * Stats[UpgradeType.Range]);
    }
}