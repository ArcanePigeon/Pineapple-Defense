using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupEnemy : Enemy
{
    public static string enemyPath = "Enemies/GroupEnemy";
    public GroupEnemy(GameScript main, GameObject enemy)
    {
        this.main = main;
        this.enemy = enemy;
        this.damage = 1;
        this.speed = 0.3f;
        this.currentSpeed = this.speed;
        this.collidableEnemy = enemy.GetComponent<CollidableEnemy>();
        collidableEnemy.Init(this);
        this.iFrameTimer = new Timer(0.1f, true);
        this.type = EnemyType.GROUP;
        this.slowDebuffTimer = new Timer(1, false);
        this.enemyStats = new EnemyStats(8, 10, 8, 2, 4, 2);
        this.isImmune = false;
    }

}
