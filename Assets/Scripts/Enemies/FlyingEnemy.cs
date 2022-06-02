using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : Enemy
{
    public static string enemyPath = "Enemies/FlyingEnemy";
    public FlyingEnemy(GameScript main, GameObject enemy)
    {
        this.main = main;
        this.enemy = enemy;
        this.damage = 1;
        this.speed = 0.5f;
        this.currentSpeed = this.speed;
        enemy.GetComponent<CollidableEnemy>().Init(this);
        this.type = EnemyType.FLYING;
        this.slowDebuffTimer = new Timer(1, false);
        this.enemyStats = new EnemyStats(15, 10, 15, 2, 8, 2);
        this.isImmune = false;
    }
}
