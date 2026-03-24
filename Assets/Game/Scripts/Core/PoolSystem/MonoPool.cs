using System.Collections.Generic;
using UnityEngine;

public class MonoPool<T> : MonoBehaviour, IObjectPool<T> where T : UnityEngine.Object
{
    [SerializeField] private T _prefab;
    [SerializeField] private int _prewarm = 10;
    private readonly Stack<T> _pool = new();
    private Transform _parent;
    private readonly HashSet<object> _inUse = new();

    private void Awake()
    {
        _parent = new GameObject(typeof(T).Name + "Pool").transform;
        _parent.SetParent(transform);
        for (int i = 0; i < _prewarm; i++)
            Release(CreateInstance());
    }

    private T CreateInstance()
    {
        var inst = Instantiate(_prefab, _parent);
        if (inst is GameObject go)
            go.SetActive(false);
        else if (inst is Component comp)
            comp.gameObject.SetActive(false);
        return inst;
    }

    public T Get()
    {
        var item = _pool.Count > 0 ? _pool.Pop() : CreateInstance();
        switch (item)
        {
            case GameObject go:
                go.SetActive(true);
                foreach (var p in go.GetComponentsInChildren<IPoolable>(true)) p.OnRent();
                _inUse.Add(go);
                break;
            case Component comp:
                var go2 = comp.gameObject; go2.SetActive(true);
                foreach (var p in go2.GetComponentsInChildren<IPoolable>(true)) p.OnRent();
                _inUse.Add(comp);
                break;
        }
        return item;
    }


    public void Release(T item)
    {
        switch (item)
        {
            case GameObject go:
                foreach (var p in go.GetComponentsInChildren<IPoolable>(true)) p.OnReturn();
                go.SetActive(false); go.transform.SetParent(_parent, true);
                _inUse.Remove(go);
                break;
            case Component comp:
                var go2 = comp.gameObject;
                foreach (var p in go2.GetComponentsInChildren<IPoolable>(true)) p.OnReturn();
                go2.SetActive(false); comp.transform.SetParent(_parent, true);
                _inUse.Remove(comp);
                break;
        }
        _pool.Push(item);
    }
}