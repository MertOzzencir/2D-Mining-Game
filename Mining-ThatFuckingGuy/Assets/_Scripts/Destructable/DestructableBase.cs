using System;
using UnityEngine;

public abstract class DestructableBase : MonoBehaviour
{
    public event Action<DestructableBase, bool> OnHit; // bool = isDead

    [SerializeField] private DestructableSO data;
    [SerializeField] private MeshFilter visualMeshFilter;
    [SerializeField] private MeshRenderer visualMeshRenderer;

    public float CurrentHealth { get; private set; }
    public DestructableSO Data => data;
    public Mesh VisualMesh => visualMeshFilter.sharedMesh;
    public Material VisualMaterial => visualMeshRenderer.sharedMaterial;

    void Awake()
    {
        CurrentHealth = data.MaxHealth;
    }

    public virtual void Destruct(float damage, out bool isDead)
    {
        isDead = false;
        CheckHealth(damage, out isDead);
    }

    public virtual void CheckHealth(float damage, out bool isDead)
    {
        isDead = false;
        CurrentHealth -= damage;

        if (CurrentHealth <= 0)
        {
            isDead = true;
            OnHit?.Invoke(this, true);
            Destroy(gameObject);
        }
        else
        {
            OnHit?.Invoke(this, false);
        }
    }

    public void SetVisualVisible(bool visible)
    {
        visualMeshRenderer.enabled = visible;
    }

    public void OnSpawned()
    {
        int randomRotation = UnityEngine.Random.Range(0, 4);
        Vector3 randomRotationVector = Vector3.zero;
        switch (randomRotation)
        {
            case 0: randomRotationVector = Vector3.zero; break;
            case 1: randomRotationVector = Vector3.up * 90; break;
            case 2: randomRotationVector = Vector3.up * 180; break;
            case 3: randomRotationVector = Vector3.up * 270; break;
        }
        transform.rotation = Quaternion.Euler(randomRotationVector);
    }
}