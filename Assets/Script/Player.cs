using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(PlayerController))]
[RequireComponent (typeof(GunController))]
public class Player : LivingEntity
{
    public float moveSpeed = 3;
    PlayerController controller;
    Camera viewCamera;
    GunController gunController;

    public Crosshairs crosshairs;
    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        gunController = GetComponent<GunController>();
        viewCamera = Camera.main;
        FindObjectOfType<Spwaner>().OnNewWave += OnNewWave;
    }
    protected override void Start()
    {
        base.Start();
    }
    void OnNewWave(int waveNumber)
    {
        gunController.EquipGun(waveNumber);

    }
    // Update is called once per frame
    void Update()
    {
        //移动
        Vector3 moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveVelocity = moveInput.normalized * moveSpeed;
        controller.Move(moveVelocity);
        //朝向
        Ray ray = viewCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.up*gunController.GunHeight);
        float distance;

        if(groundPlane.Raycast(ray,out distance))
        {
            Vector3 point = ray.GetPoint(distance);
            //Debug.DrawLine(ray.origin, point, Color.red);
            controller.LookAt(point);
            crosshairs.transform.position = point;
            crosshairs.DetectTargets(ray);
        }
        //发射子弹
        if (Input.GetMouseButton(0))
        {
            gunController.OnTriggerHold();
        }
        if (Input.GetMouseButtonUp(0))
        {
            gunController.OnTriggerRelease();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            gunController.Reload();
        }
        if (transform.position.y < -5)
        {
            TakeDamage(health);
        }
    }
    public override void Die()
    {
        AudioManager.instance.PlaySound("Player Death", transform.position);
        base.Die();
    }
}
