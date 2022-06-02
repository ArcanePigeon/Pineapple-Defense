using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliceThrowerTower : Tower
{

    public static string towerPath = "Towers/SliceThrower";
    public SliceThrowerTower(GameScript main, GameObject tower, TowerStats[] towerStats)
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
        this.type = TowerType.SLICE_THROWER;
        this.projectileType = ProjectileType.SLICE;
        this.projectileDamageReturn = new ProjectileDamageReturn(currentStats.damage, false, (float)currentStats.special);
        towerRadius.levelIndicator.text = "" + (level + 1);
        pivotSpeed = 600f;
    }

    public override void Attack()
    {
        if (closestEnemy != null)
        {
            //towerRadius.animator.ResetTrigger("Fire");
            //towerRadius.animator.SetTrigger("Fire");
            main.SpawnProjctileFromPool(projectileType, towerRadius.projectileSpawn.position, towerRadius.pivot.rotation, projectileDamageReturn);
        }
    }
}
