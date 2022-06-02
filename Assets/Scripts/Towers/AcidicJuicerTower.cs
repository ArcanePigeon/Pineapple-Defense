using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcidicJuicerTower : Tower {
    
    public static string towerPath = "Towers/AcidicJuicer" +
        "";
    public AcidicJuicerTower(GameScript main, GameObject tower, TowerStats[] towerStats)
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
        this.type = TowerType.ACIDIC_JUICER;
        this.projectileDamageReturn = new ProjectileDamageReturn(currentStats.damage, false, (float)currentStats.special);
        towerRadius.levelIndicator.text = "" + (level+1);
    }
    public override void TrackEnemies()
    {
    }
    public override void Attack()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (Vector2.Distance(targets[i].enemy.transform.position, tower.transform.position) > currentStats.range || !targets[i].enemy.activeInHierarchy)
            {
                targets.RemoveAt(i);
                i--;
                continue;
            }
            targets[i].ApplyDamage(projectileDamageReturn);
        }
    }

    public override void AddEnemyToList(Enemy enemy)
    {
        if(enemy.type == EnemyType.FLYING)
        {
            return;
        }
        targets.Add(enemy);
    }

}
