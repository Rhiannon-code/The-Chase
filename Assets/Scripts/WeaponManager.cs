using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class WeaponManager : MonoBehaviour
{
    public static WeaponManager Instance {get; set; }

    public List<GameObject> weaponSlots;
    public GameObject activeWeaponSlot;

    [Header("Ammo")]
    public int totalRifleAmmo = 0;
    public int totalPistolAmmo = 0;

    [Header("Throwables General")]
    public int flashbangs = 0;
    public int smokes = 0;
    public float throwForce = 10f;
    public GameObject throwableSpawn;
    public float forceMultiplier = 0f;
    public float forceMultiplierLimit =2f;

    [Header(" Lethals")]
    public int maxLethals = 3;
    public int lethalsCount = 0;
    public GameObject grenadePrefab;
    public Throwable.ThrowableType equippedLethalType;

    [Header("Tacticals")]
    public int maxTacticals = 3;
    public int tacticalsCount = 0;
    public Throwable.ThrowableType equippedTacticalType;
    public GameObject smokePrefab;
    public GameObject flashbangPrefab;


    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        activeWeaponSlot = weaponSlots[0];
        equippedLethalType = Throwable.ThrowableType.None;
        equippedTacticalType = Throwable.ThrowableType.None;
    }
    private void Update()
    {
        foreach (GameObject weaponSlot in weaponSlots)
        {
            if (weaponSlot == activeWeaponSlot)
            {
                weaponSlot.SetActive(true);
            }
            else
            {
                weaponSlot.SetActive(false);
            }
        }
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchActiveWeaponSlot(0);
        }
        if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchActiveWeaponSlot(1);
        } 
        if (Input.GetKey(KeyCode.G) || Input.GetKey(KeyCode.T))
        {
            forceMultiplier += Time.deltaTime;
            if (forceMultiplier > forceMultiplierLimit)
            {
                forceMultiplier = forceMultiplierLimit;
            }
        }
        if (Input.GetKeyUp(KeyCode.G))
        {
            if (lethalsCount > 0)
            {
                ThrowLethal();
            }
            forceMultiplier = 0;
        }
        if (Input.GetKeyUp(KeyCode.T))
        {
            if (tacticalsCount > 0)
            {
                ThrowTactical();
            }
            forceMultiplier = 0;
        }
           
    }

    #region  || ---- Weapon Management ---- ||
    public void PickupWeapon(GameObject pickedUpWeapon)
    {
        AddWeaponIntoActiveSlot(pickedUpWeapon);
    }

    private void AddWeaponIntoActiveSlot(GameObject pickedUpWeapon)
    {
        DropCurrentWeapon(pickedUpWeapon);
        
        pickedUpWeapon.transform.SetParent(activeWeaponSlot.transform, false);
        Weapon weapon = pickedUpWeapon.GetComponent<Weapon>();
        pickedUpWeapon.transform.localPosition = new Vector3(weapon.spawnPosition.x, weapon.spawnPosition.y, weapon.spawnPosition.z);
        pickedUpWeapon.transform.localRotation = Quaternion.Euler(weapon.spawnRotation.x, weapon.spawnRotation.y, weapon.spawnRotation.z);
        weapon.isActiveWeapon = true;
        //weapon.animator.enabled = true;
    }
    #endregion

    #region  || ---- Ammo Management ---- ||
    internal void PickupAmmo(AmmoBox ammo)
    {
        switch (ammo.ammoType)
        {
            case AmmoBox.AmmoType.PistolAmmo:
                totalPistolAmmo += ammo.ammoAmount;
                break;
            case AmmoBox.AmmoType.RifleAmmo:
                totalRifleAmmo += ammo.ammoAmount;
                break;
        }
    }
        
    private void DropCurrentWeapon(GameObject pickedUpWeapon)
    {
        if (activeWeaponSlot.transform.childCount > 0)
        {
            var weaponToDrop = activeWeaponSlot.transform.GetChild(0).gameObject;
            weaponToDrop.GetComponent<Weapon>().isActiveWeapon = false;
            //weaponToDrop.GetComponent<Weapon>().animator.enabled = false;
            weaponToDrop.transform.SetParent(pickedUpWeapon.transform.parent);
            weaponToDrop.transform.localPosition = pickedUpWeapon.transform.localPosition;
            weaponToDrop.transform.localRotation = pickedUpWeapon.transform.localRotation;
        }
    }
    public void SwitchActiveWeaponSlot(int slotNumber)
    {
        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon currentWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            currentWeapon.isActiveWeapon = false;
        }
        activeWeaponSlot = weaponSlots[slotNumber];
        if (activeWeaponSlot.transform.childCount > 0)
        {
            Weapon newWeapon = activeWeaponSlot.transform.GetChild(0).GetComponent<Weapon>();
            newWeapon.isActiveWeapon = true;
        }
    }

    internal void DecreaseTotalAmmo(int bulletsToDecrease, Weapon.WeaponModel thisWeaponModel)
    {
        switch (thisWeaponModel)
        {
            case Weapon.WeaponModel.Pistol:
                totalPistolAmmo -= bulletsToDecrease;
                break;
            case Weapon.WeaponModel.Rifle:
                totalRifleAmmo -= bulletsToDecrease;
                break;
        }
    }

    public int CheckAmmoLeftFor(Weapon.WeaponModel thisWeaponModel)
    {
        switch (thisWeaponModel)
        {
            case Weapon.WeaponModel.Pistol:
                return totalPistolAmmo;
            case Weapon.WeaponModel.Rifle:
                return totalRifleAmmo;
            default:
                return 0;
        }
    }
    #endregion

    #region  || ---- Throwable Management ---- ||
    public void PickupThrowable(Throwable throwable)
    {
        switch (throwable.throwableType)
        {
            case Throwable.ThrowableType.Grenade:
                PickupThrowableAsLethal(Throwable.ThrowableType.Grenade);
                Debug.Log("Picked up a Grenade");
                break;
            case Throwable.ThrowableType.Flashbang:
                PickupThrowableAsTactical(Throwable.ThrowableType.Flashbang);
                Debug.Log("Picked up a Flashbang");
                break;
            case Throwable.ThrowableType.Smoke:
             PickupThrowableAsTactical(Throwable.ThrowableType.Smoke);
                Debug.Log("Picked up a Smoke");
                break;
        }
    }

    private void PickupThrowableAsTactical(Throwable.ThrowableType tactical)
    {
         if (equippedTacticalType == tactical || equippedTacticalType == Throwable.ThrowableType.None)
        {
            equippedTacticalType = tactical;
            if (tacticalsCount < maxTacticals)
            {
                tacticalsCount += 1; 
                Destroy(InteractionManager.Instance.hoveredThrowable.gameObject);
                HUDManager.Instance.UpdateThrowablesUI();
            }
            else
            {
                print("Tacticals limit reached");
            }
        }
        else
        {
            //Cannot pickup different tactical type
            //Option to swap tactical type
        }
    }

    private void PickupThrowableAsLethal(Throwable.ThrowableType lethal)
    {
        if (equippedLethalType == lethal || equippedLethalType == Throwable.ThrowableType.None)
        {
            equippedLethalType = lethal;
            if (lethalsCount < maxLethals)
            {
                lethalsCount += 1; 
                Destroy(InteractionManager.Instance.hoveredThrowable.gameObject);
                HUDManager.Instance.UpdateThrowablesUI();
            }
            else
            {
                print("Lethals limit reached");
            }
        }
        else
        {
            //Cannot pickup different lethal type
            //Option to swap lethals type
        }
    }
        private void ThrowLethal()
    {
        GameObject lethalPrefab = GetThrowablePrefab(equippedLethalType);
        GameObject throwable = Instantiate(lethalPrefab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();
        rb.AddForce(Camera.main.transform.forward * throwForce * forceMultiplier, ForceMode.Impulse);
        throwable.GetComponent<Throwable>().hasBeenThrown = true;
        lethalsCount -= 1;
        
        if (lethalsCount <= 0)
        {
            equippedLethalType = Throwable.ThrowableType.None;
        }
        HUDManager.Instance.UpdateThrowablesUI();
    }
        private void ThrowTactical()
    {
        GameObject tacticalPrefab = GetThrowablePrefab(equippedTacticalType);
        GameObject throwable = Instantiate(tacticalPrefab, throwableSpawn.transform.position, Camera.main.transform.rotation);
        Rigidbody rb = throwable.GetComponent<Rigidbody>();
        rb.AddForce(Camera.main.transform.forward * throwForce * forceMultiplier, ForceMode.Impulse);
        throwable.GetComponent<Throwable>().hasBeenThrown = true;
        tacticalsCount -= 1;
        
        if (tacticalsCount <= 0)
        {
            equippedTacticalType = Throwable.ThrowableType.None;
        }
        HUDManager.Instance.UpdateThrowablesUI();
    }


    private GameObject GetThrowablePrefab(Throwable.ThrowableType throwableype)
    {
        switch (throwableype)
        {
            case Throwable.ThrowableType.Grenade:
                return grenadePrefab;
            case Throwable.ThrowableType.Smoke:
                return smokePrefab;
            case Throwable.ThrowableType.Flashbang:
                return flashbangPrefab;
        }
        return new();
    }

    #endregion
}

