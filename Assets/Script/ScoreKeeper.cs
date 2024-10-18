using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score { get;private set; }
    float lastEnemyKillTime;
    int streakCount;
    float streakExpiryTime = 1;
    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        Enermy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }

    private void OnEnemyKilled()
    {
        if(Time.time < lastEnemyKillTime+streakExpiryTime) {
            streakCount++;
        }
        else
        {
            streakCount = 0;
        }
        lastEnemyKillTime= Time.time; 
        score += 5+(int)Mathf.Pow(2,streakCount);
        //score += 5+streakCount;
    }
    //玩家死亡一次，怪物会多挂一次OnEnemyKilled
    void OnPlayerDeath()
    {
        Enermy.OnDeathStatic -= OnEnemyKilled;
    }
}
