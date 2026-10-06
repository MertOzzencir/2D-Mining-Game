using UnityEngine;

[CreateAssetMenu(fileName = "New Mining Tool Data", menuName = "Create Mining Tool Data/New Data")]
public class MiningToolSO : ToolSO
{
    public float Damage;
    public int StorageLimit;
    public float CollectAnimationTimer;
    public float ConeAngle;
}
