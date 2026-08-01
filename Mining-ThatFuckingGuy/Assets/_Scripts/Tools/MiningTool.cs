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
    private Vector3 direction;
    private float timer;
    private MiningToolSO data => Data as MiningToolSO;
    private Dictionary<DropSO, int> collectedDropsDict = new Dictionary<DropSO, int>();
    private int currentDropCollectedTotal;


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
            if (Physics.Raycast(hitRay, out RaycastHit hit, Stats[UpgradeType.Range], destructable))
            {
                if (hit.transform.TryGetComponent(out DestructableBase d))
                {
                    if (timer > Stats[UpgradeType.Cooldown])
                    {
                        d.Destruct(Stats[UpgradeType.Damage], out _);
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