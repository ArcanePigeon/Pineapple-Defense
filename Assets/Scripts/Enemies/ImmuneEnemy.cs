using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImmuneEnemy : Enemy
{
    public static string enemyPath = "Enemies/ImmuneEnemy";
    public ImmuneEnemy(GameScript main, GameObject enemy)
    {
        this.main = main;
        this.enemy = enemy;
        this.damage = 1;
        this.speed = 0.3f;
        this.currentSpeed = this.speed;
        this.collidableEnemy = enemy.GetComponent<CollidableEnemy>();
        collidableEnemy.Init(this);
        this.iFrameTimer = new Timer(0.1f, true);
        this.type = EnemyType.IMMUNE;
        this.enemyStats = new EnemyStats(5, 10, 5, 2, 4, 2);
        this.isImmune = true;
    }
}
