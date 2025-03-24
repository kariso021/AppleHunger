using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [System.Serializable]
    public class Pool
    {
        public string poolName;
        public GameObject prefab;
        public int size;             // �ʱ� ���� ���� (�ִ� ����)
        public bool hasLimit;        // ?? ���� ���� ����
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, int> maxPoolSize; // ?? ���� ������ ���� Dictionary

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        poolDictionary = new Dictionary<string, Queue<GameObject>>();
        maxPoolSize = new Dictionary<string, int>();

        foreach (var pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.poolName, objectPool);
            maxPoolSize.Add(pool.poolName, pool.hasLimit ? pool.size : int.MaxValue); // ���� ���ο� ���� ����
        }
    }

    // ? ������Ʈ ��û (�ִ� ���� ���� ����)
    public GameObject GetFromPool(string poolName, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning($"? Object Pool�� {poolName}��(��) �������� �ʽ��ϴ�.");
            return null;
        }

        Queue<GameObject> objectPool = poolDictionary[poolName];

        if (objectPool.Count == 0)
        {
            // �ִ� ���� ������ �ִ� ���
            if (poolDictionary[poolName].Count >= maxPoolSize[poolName])
            {
                GameObject oldestObject = poolDictionary[poolName].Dequeue();
                oldestObject.SetActive(false); // ���� ������ ��ü ��Ȱ��ȭ �� ����
                poolDictionary[poolName].Enqueue(oldestObject);
            }
            else
            {
                Debug.LogWarning($"? {poolName} Pool�� ���� �� (�ִ� {maxPoolSize[poolName]}��).");
                return null;
            }
        }

        GameObject obj = objectPool.Dequeue();
        obj.SetActive(true);
        //obj.transform.position = position;
        //obj.transform.rotation = rotation;
        if (parent != null) obj.transform.SetParent(parent);

        return obj;
    }

    // ? ������Ʈ ��ȯ
    public void ReturnToPool(string poolName, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning($"? Object Pool�� {poolName}��(��) �������� �ʽ��ϴ�.");
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(null);
        poolDictionary[poolName].Enqueue(obj);
    }
}
