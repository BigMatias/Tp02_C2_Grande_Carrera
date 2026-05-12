using System.Collections.Generic;
using UnityEngine;

public class GenericPool<T> where T : MonoBehaviour, IPooleable
{
    private readonly T _prefab;
    private readonly Transform _parent;
    private readonly int _maxSize;
    private readonly List<T> _all;       
    private readonly Queue<T> _inactive; 

    public int TotalCount => _all.Count;
    public int ActiveCount => _all.Count - _inactive.Count;
    public int InactiveCount => _inactive.Count;
    public int MaxSize => _maxSize;
    public bool IsFull => _all.Count >= _maxSize && _inactive.Count == 0;

    public GenericPool(T prefab, Transform parent, int initialSize, int maxSize)
    {
        _prefab = prefab;
        _parent = parent;
        _maxSize = Mathf.Max(initialSize, maxSize);
        _all = new List<T>(_maxSize);
        _inactive = new Queue<T>(_maxSize);

        Prewarm(initialSize);
    }

    public T Get()
    {
        T instance = null;

        if (_inactive.Count > 0)
        {
            instance = _inactive.Dequeue();
        }
        else if (_all.Count < _maxSize)
        {
            instance = CreateInstance();
        }
        else
        {
            return null;
        }

        instance.Activate();
        return instance;
    }

    public void Return(T instance)
    {
        if (instance == null) return;

        if (!_all.Contains(instance))
        {
            return;
        }

        instance.Deactivate();

        if (!instance.IsActive)
            _inactive.Enqueue(instance);
    }

    public void ReturnAll()
    {
        foreach (T obj in _all)
        {
            if (obj != null && obj.IsActive)
                Return(obj);
        }
    }

    private void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            T instance = CreateInstance();
            _inactive.Enqueue(instance);
        }
    }

    private T CreateInstance()
    {
        T instance = Object.Instantiate(_prefab, _parent);
        instance.gameObject.name = $"{typeof(T).Name}_{_all.Count:000}";
        instance.Deactivate();
        _all.Add(instance);
        return instance;
    }
}