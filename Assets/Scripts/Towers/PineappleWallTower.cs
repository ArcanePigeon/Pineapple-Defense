using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PineappleWallTower : Tower {
   
    public static string towerPath = "Towers/PineappleWall";
    public PineappleWallTower(GameScript main, GameObject tower, TowerStats[] towerStats)
    {
        this.main = main;
        this.tower = tower;
        this.towerStats = towerStats;
        this.currentStats = towerStats[0];
        this.upgradeStats = towerStats[1];
        this.level = 0;
        this.maxUpgrade = true;


        
        this.type = TowerType.PINEAPPLE_WALL;
    }
    public override void Tick()
    {
    }


    public override void AddEnemyToList(Enemy enemy)
    {
    }

    public override bool Upgrade()
    {
        return false;
    }

    public override void ToggleTowerLevel(bool isActive)
    {
    }
    public override void ToggleTowerRadiusDisplay(bool isActive)
    {
    }
}
