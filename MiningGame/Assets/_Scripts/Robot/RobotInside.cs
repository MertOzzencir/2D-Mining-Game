using System;
using UnityEngine;

public class RobotInside : MonoBehaviour
{
    [SerializeField] private float maxDirtStorageAmount;
    [SerializeField] private float startedFuelPercent;

    private float currentDirtStoraged;

    public event Action<bool, PlayerController> OnPlayerEnterState;
    void Awake()
    {
        currentDirtStoraged = maxDirtStorageAmount * startedFuelPercent / 100;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            OnPlayerEnterState?.Invoke(true, player);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlayerController player))
        {
            OnPlayerEnterState?.Invoke(false, player);
        }
    }
    public void SetDirtStorage(float amount)
    {

        if (currentDirtStoraged + amount <= maxDirtStorageAmount)
        {
            currentDirtStoraged += amount;
        }
        else
            currentDirtStoraged = maxDirtStorageAmount;
    }
    public void UseFuel(float usedFuel)
    {
        if (currentDirtStoraged <= 0)
        {
            currentDirtStoraged = 0;
            return;
        }
        currentDirtStoraged -= usedFuel;
    }
    public bool IsFull()
    {
        return currentDirtStoraged > 0 ? true : false;
    }

}
