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
            target.TakeDamage();

            // Cancelamos la velocidad de la bala al impactar
            // para que no empuje tanto
            Rigidbody targetRb = other.gameObject.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 knockback = other.contacts[0].normal * -1f; // dirección del impacto
                targetRb.velocity = Vector3.zero;
                targetRb.AddForce(knockback * 3f, ForceMode.Impulse); // ajusta el 3f a tu gusto
            }

            Deactivate();
        }
        Deactivate();
    }
}
