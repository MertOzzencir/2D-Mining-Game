using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericObjectPool<T> where T : Component
{
    private readonly Func<T> factory;
    public readonly Queue<T> pool = new();
    private readonly Action<T> onGet;
    private readonly Action<T> onRelease;

    public GenericObjectPool(Func<T> factory, int prewarmCount = 0, Action<T> onGet = null, Action<T> onRelease = null)
    {
        this.factory = factory;
        this.onGet = onGet;
        this.onRelease = onRelease;

        for (int i = 0; i < prewarmCount; i++)
        {
            T instance = factory();
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
        }
    }

    public T Get()
    {
        T instance = pool.Count > 0 ? pool.Dequeue() : factory();
        instance.gameObject.SetActive(true);
        onGet?.Invoke(instance);
        return instance;
    }

    public void Release(T instance)
    {
        onRelease?.Invoke(instance);
        instance.gameObject.SetActive(false);
        pool.Enqueue(instance);
    }
}