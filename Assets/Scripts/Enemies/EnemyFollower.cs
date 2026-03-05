using UnityEngine;

public class EnemyFollower : Enemy
{
    [SerializeField] private float _speed = 3f;
    [SerializeField] private int _damage = 1;
    [SerializeField] private float _damageCooldown = 1f;

    private float _lastDamageTime = 0f;
    private float _updateTargetInterval = 1f;
    private float _nextUpdateTime = 0f;

    void FixedUpdate()
    {
        // Actualizar objetivo cada segundo
        if (Time.time >= _nextUpdateTime)
        {
            FindClosestPlayer();
            _nextUpdateTime = Time.time + _updateTargetInterval;
        }

        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(lookRotation);

            Vector3 newPosition = transform.position + direction * _speed * Time.fixedDeltaTime;
            rb.MovePosition(newPosition);
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        if (Time.time < _lastDamageTime + _damageCooldown) return;

        PlayerHealth health = other.gameObject.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(_damage);
            _lastDamageTime = Time.time;
        }
    }
}