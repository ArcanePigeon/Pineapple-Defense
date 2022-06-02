using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupEnemySmall : Enemy
{
    public static string enemyPath = "Enemies/GroupEnemySmall";
    public GroupEnemySmall(GameScript main, GameObject enemy)
    {
        this.main = main;
        this.enemy = enemy;
        this.damage = 1;
        this.speed = 0.5f;
        this.currentSpeed = this.speed;
        enemy.GetComponent<CollidableEnemy>().Init(this);
        this.type = EnemyType.GROUP_SMALL;
        this.slowDebuffTimer = new Timer(1, false);
        this.enemyStats = new EnemyStats(2, 10, 2, 2, 2, 2);
        this.isImmune = false;
    }

}
