using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : Projectile
{

    public static string projectilePath = "Projectiles/Explosion";
    public Explosion(GameScript main, GameObject projectile)
    {
        this.main = main;
        this.projectile = projectile;
        this.collidableProjectile = projectile.GetComponent<CollidableProjectile>();
        this.type = ProjectileType.EXPLOSION;
        this.projectileLifetime = new Timer(0.25f, false);
    }
    public override void OnCollision()
    {
    }

    public override void Tick()
    {
        if (!projectile.activeInHierarchy)
        {
            return;
        }
        projectile.transform.localScale = projectileDamageReturn.special * Vector2.one * (1f - projectileLifetime.GetCurrentTime() * 4f);
        projectileLifetime.Tick();
        if (projectileLifetime.Status())
        {
            main.KillProjectile(this);
            return;
        }
    }

    public override void UniqueReset()
    {
        projectile.transform.localScale = Vector2.zero;
        projectileLifetime.ResetTimer();
    }
}
