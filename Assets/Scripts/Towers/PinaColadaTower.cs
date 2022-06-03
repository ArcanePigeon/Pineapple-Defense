using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinaColadaTower : Tower
{

    public static string towerPath = "Towers/PinaColada";
    public PinaColadaTower(GameScript main, GameObject tower, TowerStats[] towerStats)
    {
        this.main = main;
        this.tower = tower;
        this.towerStats = towerStats;
        currentStats = towerStats[0];
        upgradeStats = towerStats[1];
        level = 0;


        attackTimer = new Timer((float)currentStats.speed, false);
        towerRadius = tower.transform.Find("TowerRadius").GetComponent<TowerRadius>();
        towerRadius.Init(this);
        targets = new List<Enemy>();
        type = TowerType.PINA_COLADA;
        projectileType = ProjectileType.ICE;
        projectileDamageReturn = new ProjectileDamageReturn(currentStats.damage, true, (float)currentStats.special);
        towerRadius.levelIndicator.text = "" + (level + 1);
        pivotSpeed = 600f;
        upgradeTimer = new Timer(0, false);
    }
    public override void PlayAttackSound()
    {
        SoundManager.Instance.PlaySound("IceSound");
    }
}
