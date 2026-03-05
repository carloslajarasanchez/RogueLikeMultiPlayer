using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int _maxHearts = 3;
    [SerializeField] private float _iFramesDuration = 1.5f;
    [SerializeField] private float _blinkInterval = 0.1f;

    private int _currentHearts;
    private bool _isInvincible = false;
    private Renderer[] _renderers;

    // Evento para notificar a la UI
    public event System.Action<int, int> OnHealthChanged; // currentHearts, maxHearts
    public event System.Action OnDeath;

    private void Awake()
    {
        _currentHearts = _maxHearts;
        _renderers = GetComponentsInChildren<Renderer>();
    }

    private void Start()
    {
        // Notificamos a la UI al inicio para que muestre los corazones
        OnHealthChanged?.Invoke(_currentHearts, _maxHearts);
    }

    public void TakeDamage(int amount = 1)
    {
        if (_isInvincible) return;

        _currentHearts -= amount;
        _currentHearts = Mathf.Clamp(_currentHearts, 0, _maxHearts);

        OnHealthChanged?.Invoke(_currentHearts, _maxHearts);

        if (_currentHearts <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(IFramesRoutine());
    }

    public void Heal(int amount = 1)
    {
        _currentHearts += amount;
        _currentHearts = Mathf.Clamp(_currentHearts, 0, _maxHearts);
        OnHealthChanged?.Invoke(_currentHearts, _maxHearts);
    }

    private void Die()
    {
        Debug.Log("<color=red>Jugador muerto.</color>");
        OnDeath?.Invoke();
        // Aquí después añadiremos la lógica de game over
        gameObject.SetActive(false);
    }

    private IEnumerator IFramesRoutine()
    {
        _isInvincible = true;

        float elapsed = 0f;
        bool visible = true;

        while (elapsed < _iFramesDuration)
        {
            // Alternar visibilidad para el parpadeo
            visible = !visible;
            foreach (Renderer r in _renderers)
                r.enabled = visible;

            yield return new WaitForSeconds(_blinkInterval);
            elapsed += _blinkInterval;
        }

        // Aseguramos que el jugador queda visible al final
        foreach (Renderer r in _renderers)
            r.enabled = true;

        _isInvincible = false;
    }

    // Añade este método al PlayerHealth que ya tienes
    public void SetInvincible(bool invincible)
    {
        _isInvincible = invincible;

        // Si dejamos de ser invencibles nos aseguramos de quedar visibles
        if (!invincible)
        {
            foreach (Renderer r in _renderers)
                r.enabled = true;
        }
    }
}