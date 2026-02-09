using System;
using UnityEngine;

public class Throwable : MonoBehaviour
{
   [SerializeField] float delay =3f;
   [SerializeField] float damageRadius =20f;
   [SerializeField] float explosionForce =1200f;

   float countdown;

   bool hasExploded = false;
   public bool hasBeenThrown = false;

   public enum ThrowableType
   {
      None,
      Grenade,
      Flashbang,
      Smoke
   }

   public ThrowableType throwableType;

    private void Start()
    {
        countdown = delay;
    }

    private void Update()
    {
        if (hasBeenThrown)
        {
            countdown -= Time.deltaTime;
            if (countdown <= 0f && !hasExploded)
            {
                Explode();
                hasExploded = true;
            }
        }
    }

    private void Explode()
    {
        GetThrowableEffect();

        Destroy(gameObject);
    }

    private void GetThrowableEffect()
    {
        switch (throwableType)
        {
            case ThrowableType.Grenade:
                Debug.Log("Grenade exploded with radius: " + damageRadius + " and force: " + explosionForce);
                GrenadeEffect();
                break;
            case ThrowableType.Flashbang:
                Debug.Log("Flashbang exploded with radius: " + damageRadius);
                FlashBangEffect();
                break;
                case ThrowableType.Smoke:
                Debug.Log("Smoke exploded with radius: " + damageRadius);
                SmokeEffect();
                break;
            default:
                Debug.Log("Unknown throwable type");
                break;
        }
    }
    private void GrenadeEffect()
    {
        //Grenade explosion logic and visual effects
        GameObject explosionEffect = GlobalReferences.Instance.grenadeExplosionEffect;
        Instantiate(explosionEffect, transform.position, transform.rotation);

        //Grenade sound effect
        SoundManager.Instance.throwablesChannel.PlayOneShot(SoundManager.Instance.grenadeSound);
        
        //Grenade damage and force application
        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider objectInRange in colliders)
        {
            Rigidbody rb  = objectInRange.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, damageRadius);
            }
           if (objectInRange.gameObject.GetComponent<Enemy>())
            {
                objectInRange.gameObject.GetComponent<Enemy>().TakeDamage(100);
            }
        }
    }
        private void FlashBangEffect()
    {
        //Flashbang logic and visual effects
        GameObject flashbangEffect = GlobalReferences.Instance.flashBlindEffect;
        Instantiate(flashbangEffect, transform.position, transform.rotation);

        //Flashbang sound effect
        SoundManager.Instance.throwablesChannel.PlayOneShot(SoundManager.Instance.grenadeSound);
        
        //Grenade damage and force application
        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider objectInRange in colliders)
        {
            Rigidbody rb  = objectInRange.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //Apply blindness effect to enemies within radius
            }
        }
    }
    private void SmokeEffect()
    {
        //Smoke  logic and visual effects
        GameObject smokeEffect = GlobalReferences.Instance.smokePlumeEffect;
        Instantiate(smokeEffect, transform.position, transform.rotation);

        //Smoke sound effect
        SoundManager.Instance.throwablesChannel.PlayOneShot(SoundManager.Instance.grenadeSound);
        
        //Grenade damage and force application
        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider objectInRange in colliders)
        {
            Rigidbody rb  = objectInRange.GetComponent<Rigidbody>();
            if (rb != null)
            {
                //Apply block line of sight effect to enemies within radius
            }
        }
    }
}
