using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeafProjectile : Projectile
{
    
    public static string projectilePath = "Projectiles/GatlinProjectile";
    public LeafProjectile(GameScript main, GameObject projectile)
    {
        this.main = main;
        this.projectile = projectile;
        this.collidableProjectile = projectile.GetComponent<CollidableProjectile>();
        this.type = ProjectileType.LEAF;
        this.projectileLifetime = new Timer(0.2f, false);
    }
    public override void OnCollision()
    {
        main.KillProjectile(this);
    }

    public override void Tick()
    {
        if (!projectile.activeInHierarchy)
        {
            return;
        }
        projectileLifetime.Tick();
        if (projectileLifetime.Status())
        {
            main.KillProjectile(this);
            return;
        }
        projectile.transform.position = Vector2.MoveTowards(projectile.transform.position, projectile.transform.position - projectile.transform.up, 5f * Time.deltaTime);
    }

    public override void UniqueReset()
    {
        projectileLifetime.ResetTimer();
    }
}
