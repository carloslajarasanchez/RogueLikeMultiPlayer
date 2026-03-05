using System.Collections;
using UnityEngine;
using Unity.Netcode;

public abstract class Projectile : NetworkBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float _speed = 15f;
    [SerializeField] protected float _lifeTime = 3f;
    [SerializeField] protected int _damage = 1;

    protected Rigidbody _rb;
    private Vector3 _direction;
    private Collider _ownerCollider;

    // Sincronizamos la dirección en red
    private NetworkVariable<Vector3> _networkDirection = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        // Cuando la dirección cambia en red, la aplicamos localmente
        _networkDirection.OnValueChanged += OnDirectionChanged;

        // Si ya tiene dirección al spawnearse, la aplicamos
        if (_networkDirection.Value != Vector3.zero)
            _direction = _networkDirection.Value;
    }

    public override void OnNetworkDespawn()
    {
        _networkDirection.OnValueChanged -= OnDirectionChanged;
    }

    private void OnDirectionChanged(Vector3 previous, Vector3 current)
    {
        _direction = current;
    }

    protected virtual void OnEnable()
    {
        CancelInvoke();
        Invoke("Deactivate", _lifeTime);
    }

    protected virtual void Update()
    {
        if (_direction == Vector3.zero) return;
        transform.position += _direction * _speed * Time.deltaTime;
    }

    protected abstract void OnCollisionEnter(Collision other);

    public virtual void Launch(Vector3 direction, Collider ownerCollider = null)
    {
        _direction = direction.normalized;
        _ownerCollider = ownerCollider;

        // Sincronizamos la dirección a todos los clientes
        if (IsServer)
            _networkDirection.Value = direction.normalized;

        if (_rb != null)
            _rb.isKinematic = true;

        StartCoroutine(IgnoreOwnerCollision());
    }

    private IEnumerator IgnoreOwnerCollision()
    {
        if (_ownerCollider == null) yield break;

        Collider myCollider = GetComponent<Collider>();
        if (myCollider == null) yield break;

        Physics.IgnoreCollision(myCollider, _ownerCollider, true);
        yield return new WaitForSeconds(0.2f);
        Physics.IgnoreCollision(myCollider, _ownerCollider, false);
    }

    protected virtual void Deactivate()
    {
        CancelInvoke();
        _direction = Vector3.zero;
        if (IsServer) _networkDirection.Value = Vector3.zero;
        gameObject.SetActive(false);
    }

    public virtual void ResetPhysics()
    {
        _direction = Vector3.zero;
    }
}