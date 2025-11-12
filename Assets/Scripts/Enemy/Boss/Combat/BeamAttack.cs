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
    
    [Header("Audio")]
    [SerializeField] AudioClip beamChargeSound;
    [SerializeField] AudioClip beamActiveSound;
    [SerializeField] AudioClip beamEndSound;
    [SerializeField][Range(0f, 1f)] float chargeSoundVolume = 0.7f;
    [SerializeField][Range(0f, 1f)] float activeSoundVolume = 0.5f;
    [SerializeField][Range(0f, 1f)] float endSoundVolume = 0.6f;
    
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
        
        AudioSource beamAudio = beam.GetComponent<AudioSource>();
        if (beamAudio == null)
        {
            beamAudio = beam.AddComponent<AudioSource>();
            beamAudio.playOnAwake = false;
            beamAudio.spatialBlend = 0f;
        }
        
        if (beamChargeSound != null)
        {
            beamAudio.PlayOneShot(beamChargeSound, chargeSoundVolume);
        }
        
        PulseDamage dmg = beam.GetComponent<PulseDamage>();
        if (dmg == null) dmg = beam.AddComponent<PulseDamage>();
        dmg.SetDamage(damage);
        
        BeamColliderController colliderController = beam.GetComponentInChildren<BeamColliderController>();
        
        Animator anim = beam.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Play("BeamPillar");
        }
        
        if (beamActiveSound != null)
        {
            beamAudio.clip = beamActiveSound;
            beamAudio.loop = true;
            beamAudio.volume = activeSoundVolume;
            beamAudio.Play();
        }
        
        yield return new WaitForSeconds(beamDuration);
        
        if (beamAudio != null && beamAudio.isPlaying)
        {
            beamAudio.Stop();
        }
        
        if (beamEndSound != null && beamAudio != null)
        {
            beamAudio.PlayOneShot(beamEndSound, endSoundVolume);
        }
        
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