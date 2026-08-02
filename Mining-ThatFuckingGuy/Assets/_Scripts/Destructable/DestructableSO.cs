using UnityEngine;

[CreateAssetMenu(fileName = "New Destructable Data", menuName = "Create Destructable Data/New Data")]
public class DestructableSO : ScriptableObject
{
    public DestructableBase Prefab;
    public float MaxHealth;
    public ParticleBase DirtParticleVFX;
    public float DirtValue;
    public MeshFilter VisualMesh;
    public Material VisualMaterial;
    public Color Color;
}
