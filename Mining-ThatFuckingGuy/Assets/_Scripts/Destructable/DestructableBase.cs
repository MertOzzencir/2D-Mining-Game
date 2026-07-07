using System;
using UnityEngine;

public abstract class DestructableBase : MonoBehaviour
{
    public event Action<DestructableBase> OnDeath;
    [SerializeField] private DestructableSO data;
    [SerializeField] private ParticleSystem hitParticle;

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
        hitParticle.gameObject.SetActive(true);
        hitParticle.Play();

        if (CurrentHealth <= 0)
        {
            var main = hitParticle.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
            hitParticle.gameObject.transform.parent = null;

            OnDeath?.Invoke(this);
            Destroy(gameObject);
        }

    }
    public void OnSpawned()
    {
        Vector3 particleOriginalPosition = hitParticle.gameObject.transform.position;
        Quaternion lookRotation = hitParticle.gameObject.transform.rotation;
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
        hitParticle.gameObject.transform.position = particleOriginalPosition;
        hitParticle.gameObject.transform.rotation = lookRotation;

    }
}
