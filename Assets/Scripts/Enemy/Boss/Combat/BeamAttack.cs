using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BeamAttack", menuName = "Boss Attacks/Beam")]
public class BeamAttack : BaseAttack
{
    [Header("Beam Prefabs")]
    [SerializeField] GameObject beamPillarPrefab;
    
    [Header("Timing")]
    [SerializeField] float beamDuration = 3f;
    [SerializeField] float cleanupDelay = 1f;
    
    BossController bossController;
    
    public override void Initialize(Transform firePoint, Transform player)
    {
        base.Initialize(firePoint, player);
        
        if (firePoint != null)
            bossController = firePoint.GetComponentInParent<BossController>();
    }
    
    public override IEnumerator Execute()
    {
        if (beamPillarPrefab == null || firePoint == null) 
        {
            yield break;
        }
        
        if (bossController != null)
            bossController.enabled = false;
        
        GameObject beam = Object.Instantiate(beamPillarPrefab, firePoint.position, Quaternion.identity);
        
        PulseDamage dmg = beam.GetComponent<PulseDamage>();
        if (dmg == null) dmg = beam.AddComponent<PulseDamage>();
        dmg.SetDamage(damage);
        
        BeamColliderController colliderController = beam.GetComponentInChildren<BeamColliderController>();
        
        Animator anim = beam.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play("BeamPillar");
        }
        
        yield return new WaitForSeconds(beamDuration);
        
        if (colliderController != null)
        {
            colliderController.StartRising();
        }
        
        yield return new WaitForSeconds(cleanupDelay);
        
        if (beam != null) Object.Destroy(beam);
        
        if (bossController != null)
            bossController.enabled = true;
    }
}