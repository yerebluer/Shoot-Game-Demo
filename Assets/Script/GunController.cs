using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunController : MonoBehaviour
{
    public Transform weaponHold;
    public Gun[] allGuns;
    Gun equipedGun;
    private void Start()
    {
    }

    public void EquipGun(Gun gun2Equip)
    {
        if (equipedGun != null)
        {
            Destroy(equipedGun.gameObject);
        }
        equipedGun = Instantiate(gun2Equip,weaponHold.position,weaponHold.rotation) as Gun;
        equipedGun.transform.parent = weaponHold;
    }
    public void EquipGun(int weaponIndex)
    {
        //Debug.Log(weaponIndex);
        EquipGun(allGuns[weaponIndex]);
    }
    public void OnTriggerHold()
    {
        if (equipedGun != null)
        {
            equipedGun.OnTriggerHold();
        }
    }
    public void OnTriggerRelease()
    {
        if (equipedGun != null)
        {
            equipedGun.OnTriggerRelease();
        }
    }
    public void Reload()
    {
        if (equipedGun != null)
        {
            equipedGun.Reload();
        }
    } 
    public float GunHeight
    {
        get
        {
            return weaponHold.position.y;
        }
    }
}
