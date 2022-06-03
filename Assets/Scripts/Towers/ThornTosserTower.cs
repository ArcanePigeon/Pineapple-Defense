using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornTosserTower : Tower
{

    public static string towerPath = "Towers/ThornTosser";
    public ThornTosserTower(GameScript main, GameObject tower, TowerStats[] towerStats)
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
        this.type = TowerType.THORN_TOSSER;
        this.projectileType = ProjectileType.THORN;
        this.projectileDamageReturn = new ProjectileDamageReturn(currentStats.damage, false, (float)currentStats.special);
        towerRadius.levelIndicator.text = "" + (level + 1);
        upgradeTimer = new Timer(0, false);

    }
    public override void TrackEnemies()
    {
    }

    public override void Attack()
    {
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
        }
        if (targets.Count != 0)
        {
            PlayAttackSound();
            towerRadius.animator.ResetTrigger("Fire");
            towerRadius.animator.SetTrigger("Fire");
            for (int i = 0; i < 8; i++)
            {
                var angle = 45f * i;
                Quaternion projectileSpawnRotation = Quaternion.Euler(new Vector3(0, 0, angle + 90));
                Vector3 projectileSpawnPosition = towerRadius.projectileSpawn.position;
                projectileSpawnPosition += new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * 0.15f, Mathf.Sin(angle * Mathf.Deg2Rad) * 0.15f, 0);
                main.SpawnProjctileFromPool(projectileType, projectileSpawnPosition, projectileSpawnRotation, projectileDamageReturn);
            }
        }
    }
    public override void PlayAttackSound()
    {
        SoundManager.Instance.PlaySound("ThornSound");
    }
}
