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

    private bool _isInvincible = false;
    private Renderer[] _renderers;

    public event System.Action<int, int> OnHealthChanged;
    public event System.Action OnDeath;

    private void Awake()
    {
        _renderers = GetComponentsInChildren<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        _currentHearts.OnValueChanged += OnHeartsChanged;
        OnHealthChanged?.Invoke(_currentHearts.Value, _maxHearts);
    }

    public override void OnNetworkDespawn()
    {
        _currentHearts.OnValueChanged -= OnHeartsChanged;
    }

    private void OnHeartsChanged(int previous, int current)
    {
        OnHealthChanged?.Invoke(current, _maxHearts);
    }

    public void TakeDamage(int amount = 1)
    {
        if (_isInvincible) return;

        if (IsServer)
            ApplyDamage(amount);
        else
            TakeDamageServerRpc(amount);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRpc(int amount)
    {
        ApplyDamage(amount);
    }

    private void ApplyDamage(int amount)
    {
        _currentHearts.Value -= amount;
        _currentHearts.Value = Mathf.Clamp(_currentHearts.Value, 0, _maxHearts);

        if (_currentHearts.Value <= 0)
        {
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

    public void Heal(int amount = 1)
    {
        if (!IsServer) return;
        _currentHearts.Value += amount;
        _currentHearts.Value = Mathf.Clamp(_currentHearts.Value, 0, _maxHearts);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        DieClientRpc();
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        gameObject.SetActive(false);
    }

    public void SetInvincible(bool invincible)
    {
        _isInvincible = invincible;
        if (!invincible)
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