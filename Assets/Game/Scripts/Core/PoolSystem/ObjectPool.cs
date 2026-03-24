using System;
using System.Collections.Generic;

public class ObjectPool<T> : IObjectPool<T> where T : class, new()
{
    private readonly Stack<T> _pool;
    private readonly Func<T> _factory;

    public ObjectPool(int initialSize, Func<T> factory)
    {
        _factory = factory;
        _pool = new Stack<T>(initialSize);
        for (int i = 0; i < initialSize; i++)
            _pool.Push(_factory());
    }

    public T Get()
    {
        return _pool.Count > 0 ? _pool.Pop() : _factory();
    }

    public void Release(T item)
    {
        _pool.Push(item);
    }
}
