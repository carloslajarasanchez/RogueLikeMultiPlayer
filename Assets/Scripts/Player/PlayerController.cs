using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    [Header("Dash Settings")]
    [SerializeField] private float _dashSpeed = 15f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _dashCooldown = 1f;

    private Rigidbody _rigidbody;
    private Vector3 _moveInput;
    private Camera _mainCam;

    private bool _isDashing = false;
    private bool _dashOnCooldown = false;
    private Vector3 _dashDirection;

    private PlayerHealth _playerHealth;

    private int _normalLayer;
    private int _dashingLayer;

    public override void OnNetworkSpawn()
    {
        // Solo inicializamos el input si somos el jugador local
        if (!IsOwner) return;

        _rigidbody = GetComponent<Rigidbody>();
        _playerHealth = GetComponent<PlayerHealth>();

        if (_mainCam == null) _mainCam = Camera.main;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;

        _normalLayer = gameObject.layer;
        _dashingLayer = LayerMask.NameToLayer("DashingPlayer");

        // Notificamos a la cámara que el jugador local ha spawneado
        Main.CustomEvents.OnPlayerSpawned?.Invoke(transform);
    }

    void Update()
    {
        // Solo procesamos input si somos el dueño de este objeto
        if (!IsOwner) return;
        if (_isDashing) return;

        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.z = Input.GetAxisRaw("Vertical");

        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 pointToLook = ray.GetPoint(rayDistance);
            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && !_dashOnCooldown)
        {
            _dashDirection = _moveInput.normalized != Vector3.zero
                ? _moveInput.normalized
                : transform.forward;

            StartCoroutine(DashRoutine());
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner) return;

        if (_isDashing)
        {
            _rigidbody.MovePosition(_rigidbody.position + _dashDirection * _dashSpeed * Time.fixedDeltaTime);
            return;
        }

        _rigidbody.MovePosition(_rigidbody.position + _moveInput.normalized * _moveSpeed * Time.fixedDeltaTime);
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;
        _dashOnCooldown = true;

        gameObject.layer = _dashingLayer;

        if (_playerHealth != null)
            _playerHealth.SetInvincible(true);

        yield return new WaitForSeconds(_dashDuration);

        _isDashing = false;
        gameObject.layer = _normalLayer;

        if (_playerHealth != null)
            _playerHealth.SetInvincible(false);

        yield return new WaitForSeconds(_dashCooldown);
        _dashOnCooldown = false;
    }
}