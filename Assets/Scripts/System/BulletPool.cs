using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }
    
    [Header("Pool Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] int poolSize = 50;
    [SerializeField] bool autoExpand = true;
    
    Queue<GameObject> pool;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        InitializePool();
    }
    
    void InitializePool()
    {
        pool = new Queue<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewBullet();
        }
    }
    
    GameObject CreateNewBullet()
    {
        GameObject bullet = Instantiate(bulletPrefab);
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);
        pool.Enqueue(bullet);
        return bullet;
    }
    
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject bullet;
        
        if (pool.Count > 0)
        {
            bullet = pool.Dequeue();
        }
        else if (autoExpand)
        {
            Debug.LogWarning("[BulletPool] Pool exhausted, creating new bullet");
            bullet = CreateNewBullet();
            pool.Dequeue();
        }
        else
        {
            Debug.LogError("[BulletPool] Pool exhausted and auto-expand disabled!");
            return null;
        }
        
        bullet.transform.position = position;
        bullet.transform.rotation = rotation;
        bullet.SetActive(true);
        
        return bullet;
    }
    
    public void Return(GameObject bullet)
    {
        if (bullet == null) return;
        
        bullet.SetActive(false);
        bullet.transform.SetParent(transform);
        
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        
        pool.Enqueue(bullet);
    }
}