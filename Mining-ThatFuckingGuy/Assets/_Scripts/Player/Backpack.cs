using System;
using System.Collections.Generic;
using UnityEngine;

public class Backpack : MonoBehaviour
{
    [SerializeField] private float maxDirtCapacity;
    [SerializeField] private Vector2 speedRangeBySpeedReduceMuliplier;

    private float currentDirtAmount;
    private List<ParticleBase> dirtParticles = new List<ParticleBase>();

    public void AddDirt(float dirtAmount, ParticleBase breakableParticle)
    {

        if (currentDirtAmount >= maxDirtCapacity)
        {

            Destroy(breakableParticle.gameObject);
            return;
        }


        if (currentDirtAmount + dirtAmount <= maxDirtCapacity)
        {
            currentDirtAmount += dirtAmount;
            dirtParticles.Add(breakableParticle);
            breakableParticle.transform.parent = transform;
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
    public List<ParticleBase> GetParticles()
    {
        return dirtParticles;
    }
    public void ResetParticles()
    {
        dirtParticles.Clear();
    }
    public float GetSpeedReduceMultiplier()
    {
        float normalized = currentDirtAmount / maxDirtCapacity;
        return normalized * (speedRangeBySpeedReduceMuliplier.y - speedRangeBySpeedReduceMuliplier.x) + speedRangeBySpeedReduceMuliplier.x;
    }

}
