using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    //Choose fire mode
    public enum FireMode
    {
        Auto,Burst,Single
    }

    //[Tooltip("zhis is a tip")]
    public FireMode fireMode;
    public Transform[] projectileSpwan;
    public Projectile projectile;
    public float coolDownTime = 1000;
    public float muzzleVelocity = 35;
    public int burstCount;
    public int projectilesPerMag;
    public float damagePerProjectile;

    //后坐力大小，仰角
    [Header("Recoil")]
    public Vector2 kickMinMax=new Vector2(.2f,.25f);
    public Vector2 recoilAngleMinMax=new Vector2(10f,15f);
    public float recoilMoveSettleTime=.1f;
    public float recoilRotationSettleTime=.15f;

    [Header("Effects")]
    MuzzleFlash muzzleFlash;
    float nextShootTime;
    public Transform shell;
    public Transform shellEjection;

    //Audio
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    [Header("Reload")]
    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;
    int projectileRemainingInMag;
    bool isReloading;
    public float reloadTime=.5f;

    //平滑阻尼
    Vector3 recoilSmoothDampVelocity;
    float recoilAngle;
    float recoilRotSmoothDampVelocity;
    private void Start()
    {
        muzzleFlash = GetComponent<MuzzleFlash>();
        shotsRemainingInBurst = burstCount;
        projectileRemainingInMag = projectilesPerMag;
    }
    private void Update()
    {
        if(!isReloading){
            //Debug.Log(transform.localEulerAngles);
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero,ref recoilSmoothDampVelocity,recoilMoveSettleTime);
            recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVelocity, recoilRotationSettleTime);
            transform.localEulerAngles = Vector3.left * recoilAngle;
            if (projectileRemainingInMag == 0)
            {
                Reload();
            }
        }
    }
    void shoot()
    {
        if (!isReloading && Time.time > nextShootTime && projectileRemainingInMag > 0) 
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }
            for (int i = 0; i < projectileSpwan.Length; i++)
            {
                if (projectileRemainingInMag == 0)
                {
                    break;
                }
                projectileRemainingInMag--;
                Projectile newProjectile = Instantiate(projectile, projectileSpwan[i].position, projectileSpwan[i].rotation) as Projectile;
                newProjectile.SetSpeed(muzzleVelocity);
                newProjectile.SetDamage(damagePerProjectile);
            }
            nextShootTime = Time.time + coolDownTime / 1000;
            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleFlash.Activate();
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x,kickMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x,recoilAngleMinMax.y);
            //Debug.Log("before: " + recoilAngle);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);
            //Debug.Log("after: " + recoilAngle);
            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }
    public void Reload()
    {
        if (!isReloading||projectileRemainingInMag!=projectilesPerMag)
        {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
    }
    IEnumerator AnimateReload()
    {
        Debug.Log("reload");
        isReloading = true;
        yield return new WaitForSeconds(.2f);
        float percent = 0;
        float reloadSpeed = 1f / reloadTime;
        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;

        while (percent < 1)//换弹上扬，算百分比
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent) * 4;
            float reloadAngle = Mathf.Lerp(0,maxReloadAngle, interpolation);
            transform.localEulerAngles = initialRot+Vector3.left * reloadAngle;
            yield return null;
        }
        transform.localEulerAngles = initialRot;
        isReloading = false ;
        projectileRemainingInMag=projectilesPerMag;
    }
    public void OnTriggerHold()
    {
        shoot();
        triggerReleasedSinceLastShot = false;
    }
    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }
}
