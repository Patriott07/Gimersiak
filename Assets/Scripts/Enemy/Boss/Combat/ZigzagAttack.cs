using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "ZigzagAttack", menuName = "Boss Attacks/Zigzag")]
public class ZigzagAttack : BaseAttack
{
    [Header("Zigzag Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] int bulletCount = 7;
    [SerializeField] float spreadAngle = 60f;
    [SerializeField] float bulletSpeed = 8f;
    
    public override IEnumerator Execute()
    {
        if (bulletPrefab == null || firePoint == null) yield break;
        
        Vector2 dirToPlayer = GetDirectionToPlayer();
        float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;
        float startAngle = baseAngle - (spreadAngle * 0.5f);
        float angleStep = spreadAngle / Mathf.Max(1, bulletCount - 1);
        
        for (int i = 0; i < bulletCount; i++)
        {
            float currentAngle = (startAngle + angleStep * i) * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle));
            
            SpawnBullet(direction);
        }
        
        yield return null;
    }
    
    void SpawnBullet(Vector2 direction)
    {
        GameObject bullet = null;
        
        if (BulletPool.Instance != null)
            bullet = BulletPool.Instance.Get(firePoint.position, Quaternion.identity);
        else
            bullet = Object.Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        
        if (bullet == null) return;
        
        PulseDamage dmg = bullet.GetComponent<PulseDamage>();
        if (dmg != null) dmg.SetDamage(damage);
        
        ProjectileController projectile = bullet.GetComponent<ProjectileController>();
        if (projectile != null)
            projectile.Launch(direction, bulletSpeed);
    }
}