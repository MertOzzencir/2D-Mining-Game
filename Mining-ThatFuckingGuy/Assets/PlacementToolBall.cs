using System.Runtime.CompilerServices;
using UnityEngine;

public class PlacementToolBall : MonoBehaviour
{
    [SerializeField] private BallSO data;
    [SerializeField] private bool useSecondVersion;
    private Vector3 currentDirection;


    private static PlayerController player => FindAnyObjectByType<PlayerController>();

    void Update()
    {
        float moveDistance = data.Speed * Time.deltaTime;
        float checkDistance = moveDistance + 0.05f;

        if (Physics.SphereCast(transform.position, data.BallRadius, currentDirection.normalized, out RaycastHit hit, checkDistance, data.LayerMask))
        {
            bool changeDirection = false;
            if (hit.transform.TryGetComponent(out DestructableBase destruct))
            {
                destruct.Destruct(data.Damage, out changeDirection, player.transform);
            }

            if (changeDirection && useSecondVersion) return;

            currentDirection = Vector3.Reflect(currentDirection.normalized, hit.normal);
            currentDirection.x = 0;

            float randomAngle = Random.Range(-data.MaxBounceRandomAngle, data.MaxBounceRandomAngle);
            currentDirection = Quaternion.AngleAxis(randomAngle, Vector3.right) * currentDirection;


        }

        transform.position += currentDirection.normalized * moveDistance;
    }


    public void SetDirection(Vector3 direction)
    {
        currentDirection = direction;
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, currentDirection.normalized * data.RayDistance);
        Gizmos.DrawWireSphere(transform.position, data.BallRadius);
    }

}