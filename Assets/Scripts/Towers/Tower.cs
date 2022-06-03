using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TowerStats
{
    public int cost;
    public double speed;
    public int damage;
    public double range;
    public double special;
    public bool hasSpecial;
    public float upgradeTime;

    public TowerStats(int cost, double speed, int damage, double range, double special, bool hasSpecial, float upgradeTime)
    {
        this.cost = cost;
        this.speed = speed;
        this.damage = damage;
        this.range = range;
        this.special = special;
        this.hasSpecial = hasSpecial;
        this.upgradeTime = upgradeTime;
    }
}
public enum TowerType
{
    PINEAPPLE_CANNON = 0, PINA_COLADA, GATLIN_PINEAPPLE, ACIDIC_JUICER, SLICE_THROWER, THORN_TOSSER, PINEAPPLE_WALL
}
public struct TowerInfo
{
    public string name;
    public string description;
    public TowerStats towerStats;

    public TowerInfo(string name, string description, TowerStats towerStats)
    {
        this.name = name;
        this.description = description;
        this.towerStats = towerStats;
    }
}
public abstract class Tower : TickableObject
{
    public GameObject tower;
    public int level;
    public ProjectileDamageReturn projectileDamageReturn;
    public ProjectileType projectileType;
    public TowerStats[] towerStats = new TowerStats[5];
    public TowerStats currentStats;
    public TowerStats upgradeStats;
    public TowerType type;
    public TowerRadius towerRadius;
    public bool maxUpgrade;
    public Timer attackTimer;
    public List<Enemy> targets;
    public GameObject closestEnemy;
    public float pivotSpeed;
    public Timer upgradeTimer;
    public bool upgrading;
    public int totalValue;
    public virtual bool Upgrade()
    {
        if (maxUpgrade)
        {
            return false;
        }
        upgrading = true;
        upgradeTimer.SetTimerLength(upgradeStats.upgradeTime);
        upgradeTimer.ResetTimer();
        totalValue += upgradeStats.cost;
        return true;
    }
    private void UpgradeTower()
    {
        level++;
        currentStats = upgradeStats;
        if (level == 4)
        {
            maxUpgrade = true;
            upgradeStats = new TowerStats(0, 0, 0, 0, 0, currentStats.hasSpecial, 0);
        }
        else
        {
            upgradeStats = towerStats[level + 1];
        }
        attackTimer.SetTimerLength((float)currentStats.speed);
        projectileDamageReturn.damage = currentStats.damage;
        towerRadius.radius.localScale = Vector3.one * (float)currentStats.range;
        projectileDamageReturn.special = (float)currentStats.special;
        towerRadius.levelIndicator.text = "" + (level + 1);
    }
    public virtual void AddEnemyToList(Enemy enemy)
    {
        targets.Add(enemy);
    }
    public virtual void Destroy()
    {
        main.RemoveTowerFromList(this);
        main.RemoveGameObject(tower);
    }
    public TowerStats GetTowerStats()
    {
        return currentStats;
    }
    public TowerStats GetUpgradeStats()
    {
        return upgradeStats;
    }

    public virtual void ToggleTowerLevel(bool isActive)
    {
        towerRadius.towerLevel.SetActive(isActive);
    }
    public virtual void ToggleTowerRadiusDisplay(bool isActive)
    {
        towerRadius.towerRadiusDisplay.SetActive(isActive);
    }

    public override void Disable()
    {
        tower.SetActive(false);
    }
    public virtual void TrackEnemies()
    {
        closestEnemy = null;
        float closestDistance = 9999999f;
        for (int i = 0; i < targets.Count; i++)
        {
            var enemy = targets[i].enemy;
            var distance = Vector2.Distance(tower.transform.position, enemy.transform.position);
            if (!enemy.activeInHierarchy || distance > currentStats.range)
            {
                targets.RemoveAt(i);
                i--;
                continue;
            }
            if (distance < closestDistance)
            {
                closestEnemy = enemy;
            }
        }
        if (closestEnemy != null)
        {
            var target = closestEnemy.transform.position;
            var angle = Mathf.Atan2(target.y - towerRadius.pivot.position.y, target.x - towerRadius.pivot.position.x) * Mathf.Rad2Deg + 90;
            var targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            towerRadius.pivot.rotation = Quaternion.RotateTowards(towerRadius.pivot.rotation, targetRotation, pivotSpeed * Time.deltaTime);
        }
    }
    public virtual void Attack()
    {
        if (closestEnemy != null)
        {
            PlayAttackSound();
            towerRadius.animator.ResetTrigger("Fire");
            towerRadius.animator.SetTrigger("Fire");
            main.SpawnProjctileFromPool(projectileType, towerRadius.projectileSpawn.position, towerRadius.pivot.rotation, projectileDamageReturn);
        }
    }
    public virtual void PlayAttackSound()
    {

    }
    public override void Tick()
    {
        UpdateUpgradeSlider();
        TrackEnemies();
        if (!upgrading)
        {
            attackTimer.Tick();
            if (attackTimer.Status())
            {
                attackTimer.ResetTimer();
                Attack();
            }
        }

    }
    public void UpdateUpgradeSlider()
    {
        if (upgrading)
        {
            upgradeTimer.Tick();
            if (upgradeTimer.Status())
            {
                upgrading = false;
                UpgradeTower();
                main.UpdateTowerStatsCardInfo();
            }
            towerRadius.upgradeProgressSlider.value = upgradeTimer.GetTimerPercentage();
        }
    }
}
