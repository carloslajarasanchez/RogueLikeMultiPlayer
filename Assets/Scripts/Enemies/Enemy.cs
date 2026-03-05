using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))] // Obliga a que exista un Rigidbody
public abstract class Enemy : MonoBehaviour
{
    public event Action<GameObject> OnDeath;
    [SerializeField] protected int health = 3;
    protected Transform player;
    protected Rigidbody rb; // Referencia para las hijas

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    public void TakeDamage()
    {
        Debug.Log("Recibo daño");
        health--;
        if (health <= 0) Die();
    }

    public virtual void Die()
    {
        Debug.Log("Muero");
        OnDeath?.Invoke(this.gameObject);
        Destroy(this.gameObject);
    }
}