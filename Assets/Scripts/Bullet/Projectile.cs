using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float _speed = 15f;
    [SerializeField] protected float _lifeTime = 3f;
    [SerializeField] protected int _damage = 1;

    protected Rigidbody _rb;

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    protected virtual void OnEnable()
    {
        CancelInvoke();
        Invoke("Deactivate", _lifeTime);
    }

    // Ya no hay Update — el Rigidbody gestiona el movimiento

    protected abstract void OnCollisionEnter(Collision other);

    protected virtual void Deactivate()
    {
        CancelInvoke();
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        gameObject.SetActive(false);
    }

    public virtual void Launch(Vector3 direction)
    {
        _rb.velocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.AddForce(direction * _speed, ForceMode.Impulse);
    }
}