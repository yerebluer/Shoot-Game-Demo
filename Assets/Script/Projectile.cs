using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public LayerMask collisionMask;
    public LayerMask obstacleMask;
    public Color[] trailColor=new Color[4];
    float speed = 10;
    float damage = 1;

    float lifeTime = 3;
    float skinwidth = .1f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);

        //初始生成的位置就发生碰撞
        Collider[] initialCollisions = Physics.OverlapSphere(transform.position, .1f, collisionMask);
        if (initialCollisions.Length > 0)
        {
            OnHitObject(initialCollisions[0],transform.position);
        }
        //GetComponent<TrailRenderer>().material.SetColorArray("_TintColor", trailColor);
    }

    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }
    public void SetDamage(float setdamage)
    {
        damage = setdamage;
    }
    // Update is called once per frame
    void Update()
    {
        float moveDistance = speed * Time.deltaTime;
        checkCollisions(moveDistance);
        transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    private void checkCollisions(float moveDistance)
    {
        //因为物体是按帧刷新的，子弹可能在两帧之间穿过物体，不会产生实质性的碰撞，所以用ray来检测
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        //距离为moveDistance+skinwidth是因为防止敌人在向着子弹移动时，产生一段无法检测到的碰撞
        if (Physics.Raycast(ray, out hit, moveDistance+skinwidth, collisionMask, QueryTriggerInteraction.Collide))
        {
            OnHitObject(hit.collider,hit.point);
        }
        //撞到其他物体
        else if(Physics.Raycast(ray,out hit,moveDistance,obstacleMask, QueryTriggerInteraction.Collide))
        {
            Destroy(gameObject);
        }
    }
    void OnHitObject(Collider c,Vector3 hitPoint)
    {
        IDamagable damagableObject = c.GetComponent<IDamagable>();
        if (damagableObject != null)
        {
            damagableObject.TakeHit(damage,hitPoint,transform.forward);
        }
        Destroy(gameObject);
    }
}
