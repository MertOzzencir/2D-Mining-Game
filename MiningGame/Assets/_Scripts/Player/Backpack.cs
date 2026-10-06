using System;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
    [SerializeField] private float maxDirtCapacity;
    [SerializeField] private Vector2 speedRangeBySpeedReduceMuliplier;

    private float currentDirtAmount;

    public void AddDirt(float dirtAmount)
    {

        if (currentDirtAmount + dirtAmount <= maxDirtCapacity)
        {
            currentDirtAmount += dirtAmount;
        }
        else
            currentDirtAmount = maxDirtCapacity;
    }
    public bool IsEmpty()
    {
        return currentDirtAmount < maxDirtCapacity;
    }
    public float CurrentDirtAmount()
    {
        return currentDirtAmount;
    }
    public void ResetDirtInStoraged()
    {
        currentDirtAmount = 0f;
    }

    public float GetSpeedReduceMultiplier()
    {
        float normalized = currentDirtAmount / maxDirtCapacity;
        return normalized * (speedRangeBySpeedReduceMuliplier.y - speedRangeBySpeedReduceMuliplier.x) + speedRangeBySpeedReduceMuliplier.x;
    }

}
