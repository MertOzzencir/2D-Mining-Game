using System;
using UnityEngine;

public abstract class DestructableBase : MonoBehaviour
{
    public event Action<DestructableBase> OnDeath;
    [SerializeField] private DestructableSO data;
    public float CurrentHealth { get; private set; }
    void Awake()
    {
        CurrentHealth = data.MaxHealth;
    }
    public virtual void Destruct(float damage)
    {
        CheckHealth(damage);
    }
    public virtual void CheckHealth(float damage)
    {
        CurrentHealth -= damage;
        if (CurrentHealth <= 0)
        {
            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }
    }
}
