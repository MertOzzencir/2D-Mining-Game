using System;
using UnityEngine;

public abstract class DestructableBase : MonoBehaviour
{
    public event Action<DestructableBase> OnDeath;
    [SerializeField] private DestructableSO data;

    public float CurrentHealth { get; private set; }
    public DestructableSO Data => data; 

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
            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }
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