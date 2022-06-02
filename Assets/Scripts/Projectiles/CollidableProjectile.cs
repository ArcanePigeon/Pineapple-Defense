using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollidableProjectile : MonoBehaviour
{
    private Projectile projectile;
    [SerializeField] public Transform specialEffector;
    public void Init(Projectile projectile)
    {
        this.projectile = projectile;
    }
}
