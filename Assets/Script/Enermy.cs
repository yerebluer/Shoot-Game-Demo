using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enermy : LivingEntity
{
    NavMeshAgent pathfinder;
    Transform target;
    LivingEntity targetEntity;
    Material skinMaterial;

    public ParticleSystem deathEffect;
    public static event System.Action OnDeathStatic;

    Color OriginalColor;

    float attackDistanceThreshold = .5f;
    float damage = 1;
    float timeBetweenAttacks = 1;//1秒
    float nextActtackTime;

    float refreshRate = 0.25f;

    public enum State { Idle,Chasing,Attacking };
    State currentState;

    float myCollisionRadius;
    float targetCollisionRadius;

    bool hasTarget;

    private void Awake()
    {
        pathfinder = GetComponent<NavMeshAgent>();

        if (GameObject.FindGameObjectWithTag("Player") != null)
        {
            hasTarget = true;
            target = GameObject.FindGameObjectWithTag("Player").transform;
            targetEntity = target.GetComponent<LivingEntity>();

            myCollisionRadius = GetComponent<CapsuleCollider>().radius;
            targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;
        }
    }
    protected override void Start()
    {
        base.Start();

        if (hasTarget){
            currentState = State.Chasing;
            targetEntity.OnDeath += OnTargetDeath;
            //追踪
            StartCoroutine(updatePath());
        }
        
    }
    public void SetCharacteristics(float moveSpeed,int hitsToKillPlayer,float enemyHealth,Color skinColor)
    {
        pathfinder.speed = moveSpeed;
        if (hasTarget)
        {
            damage = Mathf.Ceil(targetEntity.startingHealth / hitsToKillPlayer);
        }
        startingHealth = enemyHealth;

        var main = deathEffect.main;
        main.startColor = new Color(skinColor.r, skinColor.g, skinColor.b, 1);
        Debug.Log(main.startColor);

        skinMaterial = GetComponent<Renderer>().material;
        skinMaterial.color = skinColor;
        OriginalColor = skinMaterial.color;
    }

    public override void TakeHit(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        AudioManager.instance.PlaySound("Impact", transform.position);
        if (damage >= health)
        {
            if (OnDeathStatic != null)
            {
                OnDeathStatic();
            }
            AudioManager.instance.PlaySound("Enermy Death", transform.position);
            Destroy(Instantiate(deathEffect.gameObject, hitPoint, Quaternion.FromToRotation(Vector3.forward, hitDirection)), deathEffect.main.startLifetime.constant);
        }
        base.TakeHit(damage, hitPoint, hitDirection);
    }

    void OnTargetDeath()
    {
        hasTarget = false;
        currentState = State.Idle;
    }
    // Update is called once per frame
    void Update()
    {
        if (hasTarget)
        {
            if (Time.time > nextActtackTime)
            {
                float distance = (target.position - transform.position).sqrMagnitude;
                if (distance < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2))
                {
                    nextActtackTime = Time.time + timeBetweenAttacks;
                    //StopCoroutine("Attack");
                    AudioManager.instance.PlaySound("Enermy Attack", transform.position);
                    StartCoroutine(Attack());
                }
            }
        }

    }
    IEnumerator Attack()
    {
        currentState = State.Attacking;
        pathfinder.enabled = false;
        Vector3 originalPosition = transform.position;
        Vector3 direct = (target.position - transform.position).normalized;
        Vector3 attackPosition = target.position - direct * myCollisionRadius;

        float attackSpeed = 3;
        float percent = 0;

        skinMaterial.color = Color.red;
        bool hasAppliedDamage = false;

        while (percent<=1)
        {
            if (percent >= .5f && !hasAppliedDamage)
            {
                hasAppliedDamage = true;
                targetEntity.TakeDamage(damage);
                //Debug.Log("damage!");
            }
            percent += Time.deltaTime * attackSpeed;
            float interpolation = (-Mathf.Pow(percent, 2) + percent)*4;
            transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);
            yield return null;
        }
        currentState = State.Chasing;
        skinMaterial.color = OriginalColor;
        pathfinder.enabled = true;

    }

    IEnumerator updatePath()
    {
        
        while (hasTarget)
        {
            if (currentState == State.Chasing)
            {
                Vector3 direct = (target.position - transform.position).normalized;
                Vector3 targetPosition = target.position - direct * (myCollisionRadius + targetCollisionRadius+attackDistanceThreshold/2);
                if (!dead)
                {
                    pathfinder.SetDestination(targetPosition);
                }
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }
}
