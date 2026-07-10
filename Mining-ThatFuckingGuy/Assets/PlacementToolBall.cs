using UnityEngine;

public class PlacementToolBall : MonoBehaviour
{
    [SerializeField] private BallSO data;
    Vector3[] rayDirection;
    private Vector3 currentDirection;
    void Awake()
    {
        rayDirection = new Vector3[8];
        rayDirection[0] = transform.forward;
        rayDirection[1] = -transform.forward;
        rayDirection[2] = transform.up;
        rayDirection[3] = -transform.up;
        rayDirection[4] = transform.forward + transform.up;
        rayDirection[5] = transform.forward - transform.up;
        rayDirection[6] = -transform.forward + transform.up;
        rayDirection[7] = -transform.forward - transform.up;
    }
    void Update()
    {
        foreach (var a in rayDirection)
        {
            Ray ray = new Ray(transform.position, a.normalized);
            if (Physics.Raycast(ray, out RaycastHit hit, data.RayDistance, data.LayerMask))
            {
                currentDirection = Vector3.Reflect(a.normalized, hit.normal);
                currentDirection.x = 0;
                float randomAngle = Random.Range(-data.MaxBounceRandomAngle, data.MaxBounceRandomAngle);
                currentDirection = Quaternion.AngleAxis(randomAngle, Vector3.right) * currentDirection;
                if (hit.transform.TryGetComponent(out DestructableBase destruct))
                {
                    destruct.Destruct(data.Damage);
                }
                break;
            }
        }
        transform.position += currentDirection * data.Speed * Time.deltaTime;
    }
    void OnDrawGizmos()
    {
        if (rayDirection == null) return;
        foreach (var a in rayDirection)
        {
            Gizmos.DrawRay(transform.position, a.normalized * data.RayDistance);
        }
    }
    public void SetDirection(Vector3 direction)
    {
        currentDirection = direction;
    }
}
