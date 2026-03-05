using UnityEngine;

public class EnemyBullet : Projectile
{
    protected override void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Llamamos a TakeDamage en lugar del Debug.Log anterior
            PlayerHealth health = other.gameObject.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(1);

            Rigidbody targetRb = other.gameObject.GetComponent<Rigidbody>();
            if (targetRb != null)
            {
                Vector3 knockback = other.contacts[0].normal * -1f;
                targetRb.velocity = Vector3.zero;
                targetRb.AddForce(knockback * 3f, ForceMode.Impulse);
            }

            Deactivate();
        }
        Deactivate();
    }
}