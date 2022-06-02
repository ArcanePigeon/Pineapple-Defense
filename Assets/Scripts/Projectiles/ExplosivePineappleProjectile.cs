using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosivePineappleProjectile : Projectile
{
    
    public static string projectilePath = "Projectiles/ExplosivePineapple";
    public ExplosivePineappleProjectile(GameScript main, GameObject projectile)
    {
        this.main = main;
        this.projectile = projectile;
        this.collidableProjectile = projectile.GetComponent<CollidableProjectile>();
        this.type = ProjectileType.EXPLOSIVE_PINEAPPLE;
        this.projectileLifetime = new Timer(0.5f, false);
    }
    public override void OnCollision()
    {
        main.SpawnProjctileFromPool(ProjectileType.EXPLOSION, projectile.transform.position, projectile.transform.rotation, projectileDamageReturn);
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
        projectile.transform.position = Vector2.MoveTowards(projectile.transform.position, projectile.transform.position - projectile.transform.up, 3f  * Time.deltaTime);
    }

    public override void UniqueReset()
    {
        projectileLifetime.ResetTimer();
    }
}
