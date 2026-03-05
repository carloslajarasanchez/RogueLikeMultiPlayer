using UnityEngine;

public abstract class Projectile : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float _speed = 15f;
    [SerializeField] protected float _lifeTime = 3f;
    [SerializeField] protected int _damage = 1;

    protected virtual void OnEnable()
    {
        // Al activarse desde el pool, iniciamos el temporizador para apagarse
        Invoke("Deactivate", _lifeTime);
    }

    protected virtual void Update()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    protected abstract void OnCollisionEnter(Collision other);

    protected virtual void Deactivate()
    {
        CancelInvoke();
        // En lugar de Destroy, desactivamos el objeto para que el Pool lo reutilice
        gameObject.SetActive(false);
    }

    // Método para limpiar fuerzas si usas Rigidbody
    public virtual void ResetPhysics()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}