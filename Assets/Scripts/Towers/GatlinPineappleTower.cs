using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatlinPineappleTower : Tower
{
    private Timer timeBetweenLeavesTimer;
    private int leavesLeft;

    public static string towerPath = "Towers/GatlinPineapple";
    public GatlinPineappleTower(GameScript main, GameObject tower, TowerStats[] towerStats)
    {
        this.main = main;
        this.tower = tower;
        this.towerStats = towerStats;
        this.currentStats = towerStats[0];
        this.upgradeStats = towerStats[1];
        this.level = 0;


        attackTimer = new Timer((float)currentStats.speed, false);
        towerRadius = tower.transform.Find("TowerRadius").GetComponent<TowerRadius>();
        towerRadius.Init(this);
        targets = new List<Enemy>();
        this.type = TowerType.GATLIN_PINEAPPLE;
        this.projectileType = ProjectileType.LEAF;
        this.projectileDamageReturn = new ProjectileDamageReturn(currentStats.damage, false, (float)currentStats.special);
        towerRadius.levelIndicator.text = "" + (level + 1);
        leavesLeft = 0;
        timeBetweenLeavesTimer = new Timer(0.01f, false);
        pivotSpeed = 300f;
        upgradeTimer = new Timer(0, false);
    }
    public override void Tick()
    {
        base.Tick();
        if (timeBetweenLeavesTimer.Status())
        {
            timeBetweenLeavesTimer.ResetTimer();
            if (leavesLeft > 0)
            {
                Quaternion projectileSpawnRotation = towerRadius.pivot.rotation;
                projectileSpawnRotation = Quaternion.Euler(projectileSpawnRotation.eulerAngles + new Vector3(0, 0, (Random.value - 0.5f) * 60f));
                main.SpawnProjctileFromPool(projectileType, towerRadius.projectileSpawn.position, projectileSpawnRotation, projectileDamageReturn);
                leavesLeft -= 1;
            }
        }
        else
        {
            timeBetweenLeavesTimer.Tick();
        }
    }

    public override void Attack()
    {
        if (closestEnemy != null)
        {
            towerRadius.animator.ResetTrigger("Fire");
            towerRadius.animator.SetTrigger("Fire");
            leavesLeft += (int)currentStats.special;
        }
    }
}
