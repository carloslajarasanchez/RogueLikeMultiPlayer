using UnityEngine;

public class EnemyFollower : Enemy // Hereda de Enemy
{
    [SerializeField] private float _speed = 3f;

    void FixedUpdate() // Las físicas se gestionan en FixedUpdate
    {
        if (player != null)
        {
            // Calcular dirección hacia el jugador
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Evitar que intente volar

            // Mirar al jugador
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(lookRotation);

            // Moverse usando el Rigidbody
            Vector3 newPosition = transform.position + direction * _speed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }
}