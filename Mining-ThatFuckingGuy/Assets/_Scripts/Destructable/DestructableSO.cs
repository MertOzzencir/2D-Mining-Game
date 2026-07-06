using UnityEngine;

[CreateAssetMenu(fileName = "New Destructable Data", menuName = "Create Destructable Data/New Data")]
public class DestructableSO : ScriptableObject
{
    public DestructableBase Prefab;
    public float MaxHealth;
}
