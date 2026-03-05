using System;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public abstract class Enemy : NetworkBehaviour
{
    public event Action<GameObject> OnDeath;
    [SerializeField] protected int health = 3;
    protected Transform player;
    protected Rigidbody rb;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        FindClosestPlayer();
    }

    // Busca el jugador más cercano en lugar del primero
    protected void FindClosestPlayer()
    {
        PlayerController[] players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);

        float closestDistance = float.MaxValue;
        Transform closestPlayer = null;

        foreach (PlayerController p in players)
        {
            float distance = Vector3.Distance(transform.position, p.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = p.transform;
            }
        }

        player = closestPlayer;
    }

    public void TakeDamage()
    {
        health--;
        if (health <= 0) Die();
    }

    public virtual void Die()
    {
        OnDeath?.Invoke(this.gameObject);
        Destroy(this.gameObject);
    }
}