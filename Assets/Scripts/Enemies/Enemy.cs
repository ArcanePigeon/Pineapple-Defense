using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    NORMAL = 0, IMMUNE, GROUP, FAST, FLYING, SWARM, BOSS, GROUP_SMALL, GROUP_BIG

}
public struct EnemyStats
{
    public int health;
    public float healthDifficultyMultiplier;
    public int money;
    public float moneyDifficultyMultiplier;
    public int score;
    public float scoreDifficuletMultiplier;

    public EnemyStats(int health, float healthDifficultyMultiplier, int money, float moneyDifficultyMultiplier, int score, float scoreDifficuletMultiplier)
    {
        this.health = health;
        this.healthDifficultyMultiplier = healthDifficultyMultiplier;
        this.money = money;
        this.moneyDifficultyMultiplier = moneyDifficultyMultiplier;
        this.score = score;
        this.scoreDifficuletMultiplier = scoreDifficuletMultiplier;
    }
}
public abstract class Enemy : TickableObject
{
    public int health;
    public int damage;
    public int money;
    public int score;
    public float speed;
    public float currentSpeed;
    public int difficulty;
    public GameObject enemy;
    public EnemyType type;
    public Stack<Node> path;
    public Node startingNode;
    public Node previousNode;
    public Node targetNode;
    public Vector3 target;
    public Timer slowDebuffTimer;
    public bool isSlowed;
    public EnemyStats enemyStats;
    public bool isImmune;
    public virtual void Kill(bool killedByPlayer)
    {
        main.KillEnemy(this, killedByPlayer);
    }
    public virtual int GetDamage()
    {
        return damage;
    }
    public virtual void ApplyDamage(ProjectileDamageReturn projectileDamage)
    {
        health -= projectileDamage.damage;
        if (health <= 0)
        {
            Kill(true);
        }
        if (!isImmune && projectileDamage.isSlow)
        {
            slowDebuffTimer.SetTimerLength(projectileDamage.special);
            slowDebuffTimer.ResetTimer();
            currentSpeed = speed * 0.4f;
            isSlowed = true;
        }
    }
    public virtual void OnCollision(Collider2D collision)
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
    public virtual void GenerateNewPath()
    {
        if (previousNode == null || type == EnemyType.FLYING)
        {
            return;
        }
        this.path = AStar.CheckForEnemyPath(previousNode);
        if (path.Peek() == previousNode)
        {
            path.Pop();
        }
        targetNode = path.Pop();
    }
    public override void Tick()
    {
        if (!enemy.activeInHierarchy)
        {
            return;
        }
        if (!isImmune && isSlowed)
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
    public virtual void Move()
    {
        if (path == null)
        {
            return;
        }
        if (targetNode == null && path.Count != 0)
        {
            targetNode = type == EnemyType.FLYING ? path.Peek() : path.Pop();
            target = targetNode.tile.transform.position;
        }
        //DebugPath();
        enemy.transform.position = Vector2.MoveTowards(enemy.transform.position, target, currentSpeed * Time.deltaTime);
        if (type != EnemyType.FLYING && Vector3.Distance(enemy.transform.position, target) < 0.05f)
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
    public virtual void SetEnemyStats(int difficultyLevel)
    {
        this.health = Mathf.FloorToInt(enemyStats.health + enemyStats.healthDifficultyMultiplier * Mathf.Pow(difficultyLevel, 2));
        this.money = Mathf.FloorToInt(enemyStats.money + enemyStats.moneyDifficultyMultiplier * Mathf.Pow(difficultyLevel, 2));
        this.score = Mathf.FloorToInt(enemyStats.score + enemyStats.scoreDifficuletMultiplier * Mathf.Pow(difficultyLevel, 2));
        this.isSlowed = false;
        this.currentSpeed = this.speed;
    }
    public virtual void SetEnemyPath(Stack<Node> path)
    {
        this.path = path;
    }

    public override void Disable()
    {
        enemy.SetActive(false);
    }
    public Node GetPreviousNode()
    {
        return previousNode;
    }

    public void DebugPath()
    {
        Node prev = null;
        foreach (var node in path)
        {
            if (prev == null)
            {
                prev = node;
                continue;
            }
            Debug.DrawLine(prev.tile.transform.position, node.tile.transform.position, Color.green);
            prev = node;
        }
        if (targetNode != null)
        {
            Debug.DrawLine(enemy.transform.position, targetNode.tile.transform.position, Color.red);
        }
    }
}
