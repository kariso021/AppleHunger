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
        public int size;             // 초기 생성 개수 (최대 개수)
        public bool hasLimit;        // ?? 개수 제한 여부
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;
    private Dictionary<string, int> maxPoolSize; // ?? 개수 제한을 위한 Dictionary

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
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
            maxPoolSize.Add(pool.poolName, pool.hasLimit ? pool.size : int.MaxValue); // 제한 여부에 따라 설정
        }
    }

    // ? 오브젝트 요청 (최대 개수 제한 포함)
    public GameObject GetFromPool(string poolName, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning($"? Object Pool에 {poolName}이(가) 존재하지 않습니다.");
            return null;
        }

        Queue<GameObject> objectPool = poolDictionary[poolName];

        if (objectPool.Count == 0)
        {
            // 최대 개수 제한이 있는 경우
            if (poolDictionary[poolName].Count >= maxPoolSize[poolName])
            {
                GameObject oldestObject = poolDictionary[poolName].Dequeue();
                oldestObject.SetActive(false); // 가장 오래된 객체 비활성화 후 재사용
                poolDictionary[poolName].Enqueue(oldestObject);
            }
            else
            {
                Debug.LogWarning($"? {poolName} Pool이 가득 참 (최대 {maxPoolSize[poolName]}개).");
                return null;
            }
        }

        GameObject obj = objectPool.Dequeue();

        //먼저 부모에 붙인다 (false → 로컬 위치 유지 X)
        if (parent != null)
            obj.transform.SetParent(parent, false);

        //그 다음 활성화한다!
        obj.SetActive(true);

        return obj;
    }

    // ? 오브젝트 반환
    public void ReturnToPool(string poolName, GameObject obj)
    {
        if (!poolDictionary.ContainsKey(poolName))
        {
            Debug.LogWarning($"? Object Pool에 {poolName}이(가) 존재하지 않습니다.");
            return;
        }

        obj.SetActive(false);
        obj.transform.SetParent(null);
        //혹시 scale 값이 남아 있을 경우 대비
        obj.transform.localScale = Vector3.one;

        poolDictionary[poolName].Enqueue(obj);
    }
}
