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
        this.speed = 0.5f;
        this.currentSpeed = this.speed;
        enemy.GetComponent<CollidableEnemy>().Init(this);
        this.type = EnemyType.GROUP_BIG;
        this.slowDebuffTimer = new Timer(1, false);
        this.enemyStats = new EnemyStats(6, 10, 6, 2, 3, 2);
    }

    public override void SetEnemyStats(int difficultyLevel)
    {
        this.health = Mathf.FloorToInt(enemyStats.health + enemyStats.healthDifficultyMultiplier*Mathf.Pow(difficultyLevel,2));
        this.money = Mathf.FloorToInt(enemyStats.money + enemyStats.moneyDifficultyMultiplier * Mathf.Pow(difficultyLevel, 2));
        this.score = Mathf.FloorToInt(enemyStats.score + enemyStats.scoreDifficuletMultiplier * Mathf.Pow(difficultyLevel, 2));
        this.isSlowed = false;
        this.currentSpeed = this.speed;
    }
    public override void ApplyDamage(ProjectileDamageReturn projectileDamage)
    {
        health -= projectileDamage.damage;
        if (health <= 0)
        {
            Kill(true);
        }
        if (projectileDamage.isSlow)
        {
            slowDebuffTimer.SetTimerLength(projectileDamage.special);
            slowDebuffTimer.ResetTimer();
            currentSpeed = speed * 0.4f;
            isSlowed = true;
        }
    }

    public override int GetDamage()
    {
        return damage;
    }

    public override void Kill(bool killedByPlayer)
    {
        main.KillEnemy(this, killedByPlayer);
    }

    public override void Tick()
    {
        if (!enemy.activeInHierarchy)
        {
            return;
        }
        if (isSlowed)
        {
            slowDebuffTimer.Tick();
            if (slowDebuffTimer.Status())
            {
                isSlowed = false;
                currentSpeed = speed;
            }
            
        }
        Move();
    }
    public override void Move()
    {
        if(path == null)
        {
            return;
        }
        if (targetNode == null && path.Count != 0)
        {
            targetNode = path.Pop();
            target = targetNode.tile.transform.position;
        }
        enemy.transform.position = Vector2.MoveTowards(enemy.transform.position, target, currentSpeed * Time.deltaTime);
        if(Vector3.Distance(enemy.transform.position, target) < 0.05f)
        {
            if (path.Count > 0)
            {
                previousNode = targetNode;
                targetNode = path.Pop();
                target = targetNode.tile.transform.position;
            }
        }
        var angle = Mathf.Atan2(target.y - enemy.transform.position.y, target.x - enemy.transform.position.x) * Mathf.Rad2Deg + 90;
        var targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
        enemy.transform.rotation = Quaternion.RotateTowards(enemy.transform.rotation, targetRotation, 300f * Time.deltaTime);
    }
    public override void OnCollision(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            var projectileDamage = main.CollideProjectile(collision.gameObject);
            ApplyDamage(projectileDamage);
        }
        else if (collision.CompareTag("MasterPineapple"))
        {
            Kill(false);
        }
        else if (collision.CompareTag("TowerRadius"))
        {
            collision.gameObject.GetComponent<TowerRadius>().AddEnemyToList(this);
        }
    }

    public override void GenerateNewPath()
    {
        if(previousNode == null)
        {
            return;
        }
        this.path = AStar.GetPath(previousNode.tile.pos);
        if(path.Peek() == previousNode)
        {
            path.Pop();
        }
        targetNode = path.Pop();
    }
    public override void SetEnemyPath(Stack<Node> path)
    {
        this.path = path;
    }

}
