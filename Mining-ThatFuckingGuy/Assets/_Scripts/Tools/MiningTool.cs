using System.Collections.Generic;
using UnityEngine;

public class MiningTool : ToolBase
{
    [SerializeField] private int storageLimit;
    [SerializeField] private Transform storagedPlacement;
    private Vector3 direction;
    private float timer;
    private DropBase[] storagedDrops;
    void Awake()
    {
        storagedDrops = new DropBase[storageLimit];
    }
    public override void UpdateUse()
    {
        timer += Time.deltaTime;
        direction = transform.forward;
        direction.x = 0;
        direction = direction.normalized;
        Ray hitRay = new Ray(transform.position, direction);
        if (MainUseState)
        {
            if (Physics.Raycast(hitRay, out RaycastHit hit, maxRange))
            {
                if (hit.transform.TryGetComponent(out DestructableBase d))
                {
                    if (timer > cooldownTimer)
                    {
                        d.Destruct(2);
                        timer = 0;
                    }
                }
            }
        }
        else
        {
            if (AlternativeState)
            {
                if (Physics.Raycast(hitRay, out RaycastHit hit, maxRange))
                {
                    if (hit.transform.TryGetComponent(out DropBase d))
                    {
                        for (int i = 0; i < storagedDrops.Length; i++)
                        {
                            if (storagedDrops[i] == null)
                            {
                                d.Collect(storagedPlacement);
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
