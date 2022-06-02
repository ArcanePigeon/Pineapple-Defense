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
        this.speed = 0.5f;
        this.currentSpeed = this.speed;
        enemy.GetComponent<CollidableEnemy>().Init(this);
        this.type = EnemyType.IMMUNE;
        this.enemyStats = new EnemyStats(5, 10, 5, 2, 4, 2);
        this.isImmune = true;
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
