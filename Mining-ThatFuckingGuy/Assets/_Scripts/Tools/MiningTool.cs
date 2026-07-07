using UnityEngine;

public class MiningTool : ToolBase
{
    [SerializeField] private LayerMask destructable;
    [SerializeField] private Transform visual;
    [SerializeField] private Transform aimPosition;
    [SerializeField] private int storageLimit;
    [SerializeField] private Transform storagedPlacement;
    [SerializeField] private float damage;
    private Vector3 direction;
    private float timer;
    private DropBase[] storagedDrops;
    void Awake()
    {
        storagedDrops = new DropBase[storageLimit];
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
            if (Physics.Raycast(hitRay, out RaycastHit hit, maxRange, destructable))
            {
                if (hit.transform.TryGetComponent(out DestructableBase d))
                {
                    if (timer > cooldownTimer)
                    {
                        d.Destruct(damage);
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
                                if (!d.IsCollected)
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

    void OnDrawGizmos()
    {
        Gizmos.DrawRay(aimPosition.position, aimPosition.forward * maxRange);
    }

}
