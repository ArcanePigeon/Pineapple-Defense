using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceProjectile : Projectile
{

    public static string projectilePath = "Projectiles/IceProjectile";
    public IceProjectile(GameScript main, GameObject projectile)
    {
        this.main = main;
        this.projectile = projectile;
        this.collidableProjectile = projectile.GetComponent<CollidableProjectile>();
        this.type = ProjectileType.ICE;
        this.projectileLifetime = new Timer(1f, false);
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
        projectile.transform.position = Vector2.MoveTowards(projectile.transform.position, projectile.transform.position - projectile.transform.up, 4f * Time.deltaTime);
    }

    public override void UniqueReset()
    {
        projectileLifetime.ResetTimer();
    }
}
