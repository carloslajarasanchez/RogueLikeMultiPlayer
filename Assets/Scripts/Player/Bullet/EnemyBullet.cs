using UnityEngine;

public class EnemyBullet : Projectile
{
    protected override void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Aquí llamarías a la vida del jugador
            Debug.Log("Jugador Dañado");
            Deactivate();
        }
        Deactivate();
    }
}