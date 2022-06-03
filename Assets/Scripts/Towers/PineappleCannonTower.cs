using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PineappleCannonTower : Tower
{


    public static string towerPath = "Towers/PineappleCannon";
    public PineappleCannonTower(GameScript main, GameObject tower, TowerStats[] towerStats)
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
        this.type = TowerType.PINEAPPLE_CANNON;
        this.projectileType = ProjectileType.EXPLOSIVE_PINEAPPLE;
        this.projectileDamageReturn = new ProjectileDamageReturn(currentStats.damage, false, (float)currentStats.special);
        towerRadius.levelIndicator.text = "" + (level + 1);
        pivotSpeed = 300f;
        upgradeTimer = new Timer(0, false);
    }
}
