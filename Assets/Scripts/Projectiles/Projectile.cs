using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum ProjectileType
{
    EXPLOSIVE_PINEAPPLE, ICE, LEAF, SLICE, EXPLOSION, THORN
}

public abstract class Projectile : TickableObject {
    public ProjectileDamageReturn projectileDamageReturn;
    public ProjectileType type;
    public GameObject projectile;
    public CollidableProjectile collidableProjectile;
    public Timer projectileLifetime;
    public abstract void OnCollision();
    public void SetProjectileValues(ProjectileDamageReturn projectileDamageReturn)
    {
        this.projectileDamageReturn = projectileDamageReturn;
    }
    public abstract void UniqueReset();
    public override void Disable()
    {
        projectile.SetActive(false);
    }
}
public struct ProjectileDamageReturn
{
    public int damage;
    public bool isSlow;
    public float special;

    public ProjectileDamageReturn(int damage, bool isSlow, float special)
    {
        this.damage = damage;
        this.isSlow = isSlow;
        this.special = special;
    }
}