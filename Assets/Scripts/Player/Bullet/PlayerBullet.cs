using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : Projectile
{
    protected override void OnCollisionEnter(Collision other)
    {
        Enemy target = other.gameObject.GetComponent<Enemy>();
        if (target != null)
        {
            Debug.Log("Es enemigo");
            target.TakeDamage();
            Deactivate();
        }
        Deactivate();
    }
}
