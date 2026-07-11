using UnityEngine;

[CreateAssetMenu(fileName = "New Placement Tool Ball", menuName = "Create Placement Ball/New Ball")]
public class BallSO : ScriptableObject
{
    public LayerMask LayerMask;
    public float RayDistance;
    public float Speed;
    public float MaxBounceRandomAngle;
    public int Damage;
    public float BallRadius;
}
