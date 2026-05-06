using System.Collections.Generic;
using UnityEngine;

public class MyPoolManager : MonoBehaviour
{
   /* [SerializeField] private GameDataSO gameDataSO;
    [SerializeField] private CitizenDataSO citizenDataSO;
    [SerializeField] private GameObject spawnPointsHolder;
    [SerializeField] private GameObject enemy;
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform enemiesParent;
    [SerializeField] private GameObject civilian;
    [SerializeField] private Transform civilianParent;
    [SerializeField] private Transform bulletContainer;
    [SerializeField] private List<MonoBehaviour> prefabs = new List<MonoBehaviour>();

    private Queue<GameObject> bulletPool = new Queue<GameObject>();
    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private Queue<GameObject> civilianPool = new Queue<GameObject>();
    private Transform[] spawnPoints;
    private Dictionary<Type, List<IPooleable>> poolablesDictionary;

    void Start()
    {
        poolablesDictionary.Add(typeof(Bullet), new List<IPooleable>());
        for (int i = 0; i < spawnPointsHolder.transform.childCount; i++)
        {
            spawnPoints[i] = spawnPointsHolder.transform.GetChild(i);
        }
        enemyPool = new Queue<GameObject>();
        civilianPool = new Queue<GameObject>();
        bulletPool = new Queue<GameObject>();

        InitializePool();
        InstantiateBullets();
    }

    private void InitializePool()
    {
        foreach(KeyValuePair<Type, List<IPooleable>> Item in poolablesDictionary)
        {
            MonoBehaviour prefab = FindPrefab(Item.Key);
            if (prefab != null)
            {
                CreatePool(prefab.gameObject, this.transform, 10, Item.Value);
            }
        }
    }

    public void CreatePool(GameObject prefab, Transform parent, int quantity, List<IPooleable> list)
    {
        for(int i = 0; i < quantity; i++)
        {
            GameObject go = Instantiate(prefab);
            go.transform.parent = parent;
            IPooleable poolable = go.GetComponent<IPooleable>();
            poolable.DeActivate();
            list.Add(poolable);
        }
    }

    private MonoBehaviour FindPrefab()
    {
        MonoBehaviour prefab = null;
    }

    public T GetInstanceFromPool<T> () where T : MonoBehaviour
    {
        foreach (KeyValuePair<Type, List<IPooleable>> item in poolablesDictionary)
        {
            if(typeof(T) == item.Key)
            {
                for(int i = 0; i < item.Value.Count; i++)
                {
                    if (!item.Value[i].isactive)
                    {
                        item.Value[i].Activate();
                        return (T).item.Value[i];
                    }
                }
            }
        }
    }

    private void InstantiateBullets()
    {
        for (int i = 0; i < citizenDataSO.BulletInstantiateQuantity; i++)
        {
            GameObject obj = Instantiate(bullet, bulletContainer);
            obj.SetActive(false);
            bulletPool.Enqueue(obj);
        }
    }

    public void ReturnEnemyToPool(GameObject obj)
    {
        enemyPool.Enqueue(obj);
        obj.SetActive(false);
    }

    public void ReturnCivilianToPool(GameObject obj)
    {
        civilianPool.Enqueue(obj);
        obj.SetActive(false);
    }

    public void ReturnBulletToPool(GameObject obj)
    {
        bulletPool.Enqueue(obj);
        obj.SetActive(false);
    }

    public GameObject GetBullet()
    {
        GameObject bullet = bulletPool.Dequeue();
        return bullet;
    }*/

}
