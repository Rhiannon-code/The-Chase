using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Weapon : MonoBehaviour
{

public bool isActiveWeapon;
public int weaponDamage;
public bool isShooting, readyToShoot;
bool allowReset = true;
public float shootingDelay= 2f;
public int bulletsPerBurst = 3;
public int burstBulletsLeft;
public float spreadIntensity;
public GameObject bulletPrefab;
public Transform bulletSpawn;
public float bulletVelocity = 30;
public float bulletPrefabLifeTime = 3f;

public GameObject muzzleEffect;
//internal Animator animator;


public float reloadTime;
public int magazineSize, bulletsLeft;
public bool isReloading;

public Vector3 spawnPosition;
public Vector3 spawnRotation;

public enum WeaponModel
    {
        Pistol,
        Rifle
    }
    public WeaponModel thisWeaponModel;

public enum ShootingMode
    {
        Single,
        Burst,
        Auto
    }
    public ShootingMode currentShootingMode;
    private void Awake()
    {
       readyToShoot = true;
       burstBulletsLeft = bulletsPerBurst;
       //animator = GetComponent<Animator>(); 
       bulletsLeft = magazineSize;
    }
    void Update()
    {
        if (isActiveWeapon)
        {
            foreach (Transform child in transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("WeaponRender");
            }
        GetComponent<Outline>().enabled = false;
        if (bulletsLeft ==0 && isShooting)
        {
            SoundManager.Instance.emptyMagazineSoundPistol.Play();
        }
       if (currentShootingMode == ShootingMode.Auto)
        {
            isShooting = Input.GetKey(KeyCode.Mouse0);
        }
        else if (currentShootingMode == ShootingMode.Single || currentShootingMode == ShootingMode.Burst)
        {
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);
        }
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && isReloading == false && WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > 0)
        {
            ReloadWeapon();
        }
        //Auomatic Reload When Out of Bullets
        if (readyToShoot && !isShooting && isReloading == false && bulletsLeft <= 0)
        {
            //ReloadWeapon();
        }
        if (readyToShoot && isShooting && bulletsLeft > 0)
        {
            burstBulletsLeft = bulletsPerBurst;
            FireWeapon();
        }
       
        }
        else
        {
                    foreach (Transform child in transform)
            {
                child.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }
    }
    private void FireWeapon()
    {
        bulletsLeft--;
        muzzleEffect.GetComponent<ParticleSystem>().Play();
        //animator.SetTrigger("RECOIL");
        SoundManager.Instance.PlayShootingSound(thisWeaponModel);

        readyToShoot = false;
        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        Bullet bul = bullet.GetComponent<Bullet>();
        bul.bulletDamage = weaponDamage;
        bullet.transform.forward = shootingDirection;
        bullet.GetComponent<Rigidbody>().AddForce(shootingDirection * bulletVelocity, ForceMode.Impulse);
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));
        if (allowReset)
        {
            Invoke("ResetShot", shootingDelay);
            allowReset = false;
        }
        //Check if we are in Burst Mode
        if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1)
        {
            burstBulletsLeft--;
            Invoke("FireWeapon", shootingDelay); 
        }
    }
    
    private void ReloadWeapon()
    {
        SoundManager.Instance.PlayReloadingSound(thisWeaponModel);
        //animator.SetTrigger("RELOAD");
        isReloading = true;
        Invoke("ReloadingCompleted", reloadTime);
    }

    private void ReloadingCompleted()
    {
        if (WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel) > magazineSize)
        {
            bulletsLeft = magazineSize;
            WeaponManager.Instance.DecreaseTotalAmmo(bulletsLeft, thisWeaponModel);
        }
        else
        {
            bulletsLeft = WeaponManager.Instance.CheckAmmoLeftFor(thisWeaponModel);
            WeaponManager.Instance.DecreaseTotalAmmo(bulletsLeft, thisWeaponModel);
        }
        isReloading = false;
    }
    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }
    public Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 targetPoint;  
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(1000); 
        }
        Vector3 direction = targetPoint - bulletSpawn.position;
        float x = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        return direction + new Vector3(x, y, 0);
    }
    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
