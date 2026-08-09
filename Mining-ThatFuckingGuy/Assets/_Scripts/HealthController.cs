using UnityEngine;

public class HealthController : MonoBehaviour
{
    [SerializeField] private float maxHealth;

    private float currentHealth;
    private bool isDead;
    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;
        Debug.Log("Counter Health");
        currentHealth -= damage;
        CheckDeath();
    }
    public void CheckDeath()
    {
        if (currentHealth <= 0)
        {
            isDead = true;
            Destroy(gameObject);
        }
    }
}
