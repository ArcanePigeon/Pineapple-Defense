using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : Enemy
{
    public static string enemyPath = "Enemies/BossEnemy";
    public BossEnemy(GameScript main, GameObject enemy)
    {
        this.main = main;
        this.enemy = enemy;
        this.damage = 1;
        this.speed = 0.3f;
        this.currentSpeed = this.speed;
        this.collidableEnemy = enemy.GetComponent<CollidableEnemy>();
        collidableEnemy.Init(this);
        this.iFrameTimer = new Timer(0.1f, true);
        this.type = EnemyType.BOSS;
        this.slowDebuffTimer = new Timer(1, false);
        this.enemyStats = new EnemyStats(100, 30, 100, 2, 10, 2);
        this.isImmune = false;
    }

}
