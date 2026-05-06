using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [Serializable]
    public class PoolConfig
    {
        [Tooltip("Nombre descriptivo para el contenedor en la jerarquía (ej. 'Bullets', 'Enemies').")]
        public string poolName = "Pool";

        [Tooltip("Prefab del objeto. DEBE tener un componente que implemente IPooleable.")]
        public GameObject prefab;

        [Tooltip("Cantidad de objetos preinstaciados al inicio.")]
        [Min(1)] public int initialSize = 10;

        [Tooltip("Máximo de objetos simultáneos. Get() devuelve null si se supera.")]
        [Min(1)] public int maxSize = 20;
    }

    public static PoolManager Instance { get; private set; }

    [Header("Configuración de Pools")]
    [SerializeField] private PoolConfig[] poolConfigs;

    private Dictionary<Type, IPool> _pools = new Dictionary<Type, IPool>();

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
            Debug.LogWarning("[PoolManager] No hay configuraciones de pool asignadas.");
            return;
        }

        foreach (PoolConfig config in poolConfigs)
        {
            if (config.prefab == null)
            {
                Debug.LogWarning("[PoolManager] Un PoolConfig tiene prefab null. Ignorado.");
                continue;
            }

            IPooleable pooleable = config.prefab.GetComponent<IPooleable>();
            if (pooleable == null)
            {
                Debug.LogError($"[PoolManager] El prefab '{config.prefab.name}' no tiene un " +
                               "componente que implemente IPooleable. Pool no creado.");
                continue;
            }

            Type componentType = pooleable.GetType();

            if (_pools.ContainsKey(componentType))
            {
                Debug.LogWarning($"[PoolManager] Ya existe un pool para el tipo '{componentType.Name}'. " +
                                  "Ignorando duplicado.");
                continue;
            }

            Transform container = CreateContainer(config.poolName);

            IPool pool = PoolFactory.Create(componentType, config.prefab, container,
                                            config.initialSize, config.maxSize);

            if (pool != null)
                _pools.Add(componentType, pool);
        }

        Debug.Log($"[PoolManager] {_pools.Count} pools inicializados.");
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
            Debug.LogError($"[PoolManager] No existe pool para el tipo '{type.Name}'. " +
                            "¿Está registrado en poolConfigs?");
            return null;
        }

        return (pool as PoolWrapper<T>)?.Get();
    }
    public void Return<T>(T instance) where T : MonoBehaviour, IPooleable
    {
        Type type = typeof(T);

        if (!_pools.TryGetValue(type, out IPool pool))
        {
            Debug.LogError($"[PoolManager] No existe pool para el tipo '{type.Name}'.");
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

