using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BurstAttack", menuName = "Boss Attacks/Burst")]
public class BurstAttack : BaseAttack
{
    [Header("Burst Settings")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] int rounds = 3;
    [SerializeField] float delayBetweenRounds = 0.15f;
    [SerializeField] float bulletSpeed = 12f;
    [SerializeField] float randomAngleVariance = 5f;
    
    public override IEnumerator Execute()
    {
        if (bulletPrefab == null || firePoint == null) yield break;
        
        for (int i = 0; i < rounds; i++)
        {
            Vector2 dirToPlayer = GetDirectionToPlayer();
            float randomOffset = Random.Range(-randomAngleVariance, randomAngleVariance);
            float angle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) + randomOffset * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            
            SpawnBullet(direction);
            
            if (i < rounds - 1)
                yield return new WaitForSeconds(delayBetweenRounds);
        }
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