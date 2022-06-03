using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastEnemy : Enemy
{
    public static string enemyPath = "Enemies/FastEnemy";
    public FastEnemy(GameScript main, GameObject enemy)
    {
        this.main = main;
        this.enemy = enemy;
        this.damage = 1;
        this.speed = 0.8f;
        this.currentSpeed = this.speed;
        this.collidableEnemy = enemy.GetComponent<CollidableEnemy>();
        collidableEnemy.Init(this);
        this.iFrameTimer = new Timer(0.1f, true);
        this.type = EnemyType.FAST;
        this.slowDebuffTimer = new Timer(1, false);
        this.enemyStats = new EnemyStats(9, 10, 9, 2, 5, 2);
        this.isImmune = false;
    }

}
