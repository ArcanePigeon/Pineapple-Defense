using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupEnemyBig : Enemy
{
    public static string enemyPath = "Enemies/GroupEnemyBig";
    public GroupEnemyBig(GameScript main, GameObject enemy)
    {
        this.main = main;
        this.enemy = enemy;
        this.damage = 1;
        this.speed = 0.2f;
        this.currentSpeed = this.speed;
        this.collidableEnemy = enemy.GetComponent<CollidableEnemy>();
        collidableEnemy.Init(this);
        this.iFrameTimer = new Timer(0.1f, true);
        this.type = EnemyType.GROUP_BIG;
        this.slowDebuffTimer = new Timer(1, false);
        this.enemyStats = new EnemyStats(6, 10, 6, 2, 3, 2);
        this.isImmune = false;
    }

}
