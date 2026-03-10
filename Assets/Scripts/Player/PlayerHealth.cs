using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerHealth : NetworkBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int _maxHearts = 3;
    [SerializeField] private float _iFramesDuration = 1.5f;
    [SerializeField] private float _blinkInterval = 0.1f;

    private NetworkVariable<int> _currentHearts = new NetworkVariable<int>(
        3,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private NetworkVariable<bool> _isDead = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private bool _isInvincible = false;
    private Renderer[] _renderers;
    private bool _killedByPlayer = false;
    private Vector3 _deathPosition;
    private int _normalLayer;
    private int _deadLayer;

    public event System.Action<int, int> OnHealthChanged;
    public event System.Action OnDeath;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _normalLayer = gameObject.layer;
        _deadLayer = LayerMask.NameToLayer("DeadPlayer");
    }

    public override void OnNetworkSpawn()
    {
        _currentHearts.OnValueChanged += OnHeartsChanged;
        _isDead.OnValueChanged += OnDeadChanged;
        OnHealthChanged?.Invoke(_currentHearts.Value, _maxHearts);
    }

    public override void OnNetworkDespawn()
    {
        _currentHearts.OnValueChanged -= OnHeartsChanged;
        _isDead.OnValueChanged -= OnDeadChanged;
    }

    private void OnHeartsChanged(int previous, int current)
    {
        OnHealthChanged?.Invoke(current, _maxHearts);
    }

    private void OnDeadChanged(bool previous, bool current)
    {
        // Cambia layer — DeadPlayer no colisiona con nada
        gameObject.layer = current ? _deadLayer : _normalLayer;

        // Oculta/muestra
        foreach (Renderer r in _renderers)
            r.enabled = !current;

        // Bloquea movimiento solo en el dueño
        if (IsOwner)
        {
            PlayerController pc = GetComponent<PlayerController>();
            if (pc != null) pc.SetMovementEnabled(!current);
        }
    }

    public void TakeDamage(int amount = 1, bool killedByPlayer = false)
    {
        if (_isInvincible) return;
        if (_isDead.Value) return;

        if (IsServer)
            ApplyDamage(amount, killedByPlayer);
        else
            TakeDamageServerRpc(amount, killedByPlayer);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int amount, bool killedByPlayer)
    {
        ApplyDamage(amount, killedByPlayer);
    }

    private void ApplyDamage(int amount, bool killedByPlayer = false)
    {
        if (_isDead.Value) return;

        _currentHearts.Value -= amount;
        _currentHearts.Value = Mathf.Clamp(_currentHearts.Value, 0, _maxHearts);

        if (_currentHearts.Value <= 0)
        {
            _killedByPlayer = killedByPlayer;
            Die();
            return;
        }

        TriggerIFramesClientRpc();
    }

    [ClientRpc]
    private void TriggerIFramesClientRpc()
    {
        if (!IsOwner) return;
        StartCoroutine(IFramesRoutine());
    }

    public void ResetHealth()
    {
        if (!IsServer) return;
        _currentHearts.Value = _maxHearts;
        _isDead.Value = false;
    }

    public void Heal(int amount = 1)
    {
        if (!IsServer) return;
        _currentHearts.Value += amount;
        _currentHearts.Value = Mathf.Clamp(_currentHearts.Value, 0, _maxHearts);
    }

    private void Die()
    {
        if (_isDead.Value) return;

        _deathPosition = transform.position;
        _isDead.Value = true;
        OnDeath?.Invoke();

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.RequestRespawn(
                OwnerClientId, _deathPosition, _killedByPlayer);
    }

    public void TriggerRespawnIFrames()
    {
        if (!IsOwner) return;
        StartCoroutine(IFramesRoutine());
    }

    public void SetInvincible(bool invincible)
    {
        _isInvincible = invincible;
        if (!invincible && !_isDead.Value)
        {
            foreach (Renderer r in _renderers)
                r.enabled = true;
        }
    }

    private IEnumerator IFramesRoutine()
    {
        _isInvincible = true;
        float elapsed = 0f;
        bool visible = true;

        while (elapsed < _iFramesDuration)
        {
            visible = !visible;
            foreach (Renderer r in _renderers)
                r.enabled = visible;

            yield return new WaitForSeconds(_blinkInterval);
            elapsed += _blinkInterval;
        }

        foreach (Renderer r in _renderers)
            r.enabled = true;

        _isInvincible = false;
    }
}