using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [Serializable]
    public class PoolConfig
    {
        [Tooltip("Nombre Pool: ")]
        public string poolName = "Pool";

        [Tooltip("Prefab: ")]
        public GameObject prefab;

        [Tooltip("Cantidad Inicial: ")]
        [Min(1)] public int initialSize = 10;

        [Tooltip("Max: ")]
        [Min(1)] public int maxSize = 20;
    }

    [Header("Configuración: ")]
    [SerializeField] private PoolConfig[] poolConfigs;

    private Dictionary<Type, IPool> _pools = new Dictionary<Type, IPool>();
    public static PoolManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); 

        InitializeAllPools();
    }

    private void InitializeAllPools()
    {
        if (poolConfigs == null || poolConfigs.Length == 0)
        {
            return;
        }

        foreach (PoolConfig config in poolConfigs)
        {
            if (config.prefab == null)
            {
                continue;
            }

            IPooleable pooleable = config.prefab.GetComponent<IPooleable>();
            if (pooleable == null)
            {
                continue;
            }

            Type componentType = pooleable.GetType();

            if (_pools.ContainsKey(componentType))
            {
                continue;
            }

            Transform container = CreateContainer(config.poolName);

            IPool pool = PoolFactory.Create(componentType, config.prefab, container,
                                            config.initialSize, config.maxSize);

            if (pool != null)
                _pools.Add(componentType, pool);
        }

    }

    private Transform CreateContainer(string poolName)
    {
        GameObject container = new GameObject($"[Pool] {poolName} Content");
        container.transform.SetParent(this.transform);
        return container.transform;
    }

    public T Get<T>() where T : MonoBehaviour, IPooleable
    {
        Type type = typeof(T);

        if (!_pools.TryGetValue(type, out IPool pool))
        {
            return null;
        }

        return (pool as PoolWrapper<T>)?.Get();
    }
    public void Return<T>(T instance) where T : MonoBehaviour, IPooleable
    {
        Type type = typeof(T);

        if (!_pools.TryGetValue(type, out IPool pool))
        {
            return;
        }

        (pool as PoolWrapper<T>)?.Return(instance);
    }

    public void ReturnAll<T>() where T : MonoBehaviour, IPooleable
    {
        Type type = typeof(T);
        if (_pools.TryGetValue(type, out IPool pool))
            (pool as PoolWrapper<T>)?.ReturnAll();
    }

    public void ReturnAllPools()
    {
        foreach (IPool pool in _pools.Values)
            pool.ReturnAll();
    }
    public bool IsPoolFull<T>() where T : MonoBehaviour, IPooleable
    {
        Type type = typeof(T);
        if (_pools.TryGetValue(type, out IPool pool))
            return pool.IsFull;
        return true;
    }

    public int ActiveCount<T>() where T : MonoBehaviour, IPooleable
    {
        Type type = typeof(T);
        if (_pools.TryGetValue(type, out IPool pool))
            return pool.ActiveCount;
        return 0;
    }
}

public interface IPool
{
    bool IsFull { get; }
    int ActiveCount { get; }
    void ReturnAll();
}

public class PoolWrapper<T> : IPool where T : MonoBehaviour, IPooleable
{
    private readonly GenericPool<T> _inner;

    public PoolWrapper(T prefab, Transform parent, int initialSize, int maxSize)
    {
        _inner = new GenericPool<T>(prefab, parent, initialSize, maxSize);
    }

    public bool IsFull => _inner.IsFull;
    public int ActiveCount => _inner.ActiveCount;
    public void ReturnAll() => _inner.ReturnAll();
    public T Get() => _inner.Get();
    public void Return(T obj) => _inner.Return(obj);
}

public static class PoolFactory
{
    public static IPool Create(Type componentType, GameObject prefab,
                               Transform parent, int initialSize, int maxSize)
    {
        MonoBehaviour component = prefab.GetComponent(componentType) as MonoBehaviour;
        if (component == null) return null;

        Type wrapperType = typeof(PoolWrapper<>).MakeGenericType(componentType);
        IPool pool = (IPool)Activator.CreateInstance(
            wrapperType,
            component,   
            parent,      
            initialSize, 
            maxSize      
        );

        return pool;
    }
}

