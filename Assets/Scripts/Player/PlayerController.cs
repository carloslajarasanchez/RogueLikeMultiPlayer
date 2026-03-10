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
    private bool _movementEnabled = true;
    private RoomController _currentRoom = null;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
            StartCoroutine(InitDelayed());
        NotifySpawnedClientRpc();
    }

    private IEnumerator InitDelayed()
    {
        yield return null;
        _rigidbody = GetComponent<Rigidbody>();
        _playerHealth = GetComponent<PlayerHealth>();
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX
                               | RigidbodyConstraints.FreezeRotationZ;
        _normalLayer = gameObject.layer;
        _dashingLayer = LayerMask.NameToLayer("DashingPlayer");
        _mainCam = Camera.main;
        Main.CustomEvents.OnLocalPlayerSpawned?.Invoke(transform);
    }

    [ClientRpc]
    private void NotifySpawnedClientRpc()
    {
        StartCoroutine(NotifyDelayed());
    }

    private IEnumerator NotifyDelayed()
    {
        yield return null;
        yield return null;
        Main.CustomEvents.OnAnyPlayerSpawned?.Invoke(transform);
    }

    public void SetMovementEnabled(bool enabled)
    {
        _movementEnabled = enabled;
        if (!enabled && _rigidbody != null)
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        RoomController room = other.GetComponentInParent<RoomController>();
        if (room == null) return;
        if (room == _currentRoom) return;

        _currentRoom = room;
        // Pasamos la posición del RoomController, no del trigger
        NotifyRoomEnteredServerRpc(room.transform.position);
    }

    [ServerRpc]
    private void NotifyRoomEnteredServerRpc(Vector3 roomPosition)
    {
        Debug.Log($"<color=magenta>ServerRpc recibido. Buscando sala en {roomPosition}</color>");
        RoomController[] allRooms = FindObjectsByType<RoomController>(FindObjectsSortMode.None);
        Debug.Log($"<color=magenta>Salas encontradas en servidor: {allRooms.Length}</color>");

        RoomController closest = null;
        float minDist = float.MaxValue;

        foreach (var room in allRooms)
        {
            float dist = Vector3.Distance(room.transform.position, roomPosition);
            Debug.Log($"<color=magenta>Sala {room.gameObject.name} dist={dist}</color>");
            if (dist < minDist)
            {
                minDist = dist;
                closest = room;
            }
        }

        Debug.Log($"<color=magenta>Sala más cercana: {closest?.gameObject.name} dist={minDist}</color>");
        if (closest != null)
            closest.TryActivateRoom();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (!_movementEnabled) return;
        if (_isDashing) return;

        if (_mainCam == null)
        {
            _mainCam = Camera.main;
            return;
        }

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
        if (_rigidbody == null) return;
        if (!_movementEnabled) return;

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