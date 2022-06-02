using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornProjectile : Projectile
{

    public static string projectilePath = "Projectiles/ThornProjectile";
    private int collisionsUntilBreak;
    public ThornProjectile(GameScript main, GameObject projectile)
    {
        this.main = main;
        this.projectile = projectile;
        this.collidableProjectile = projectile.GetComponent<CollidableProjectile>();
        this.type = ProjectileType.SLICE;
        this.projectileLifetime = new Timer(1f, false);
    }
    public override void OnCollision()
    {
        if (collisionsUntilBreak > 0)
        {
            collisionsUntilBreak -= 1;
            return;
        }
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
        projectile.transform.position = Vector2.MoveTowards(projectile.transform.position, projectile.transform.position - projectile.transform.up, 2.5f * Time.deltaTime);
    }

    public override void UniqueReset()
    {
        collisionsUntilBreak = (int)projectileDamageReturn.special;
        projectileLifetime.ResetTimer();
    }
}
