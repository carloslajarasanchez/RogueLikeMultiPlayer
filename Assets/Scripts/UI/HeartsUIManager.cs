using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartsUIManager : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite _heartFull;
    [SerializeField] private Sprite _heartEmpty;

    [Header("Paneles de UI por jugador")]
    [SerializeField] private GameObject _localPanel;
    [SerializeField] private GameObject _player2Panel;
    [SerializeField] private GameObject _player3Panel;
    [SerializeField] private GameObject _player4Panel;

    [Header("Corazones - Jugador Local")]
    [SerializeField] private Image[] _localHearts;

    [Header("Corazones - Jugador 2")]
    [SerializeField] private Image[] _player2Hearts;

    [Header("Corazones - Jugador 3")]
    [SerializeField] private Image[] _player3Hearts;

    [Header("Corazones - Jugador 4")]
    [SerializeField] private Image[] _player4Hearts;

    private Dictionary<ulong, Image[]> _clientHearts = new Dictionary<ulong, Image[]>();
    private Dictionary<ulong, PlayerHealth> _clientHealth = new Dictionary<ulong, PlayerHealth>();

    private void Awake()
    {
        Main.CustomEvents.OnAnyPlayerSpawned.AddListener(OnPlayerSpawned);

        _player2Panel.SetActive(false);
        _player3Panel.SetActive(false);
        _player4Panel.SetActive(false);
    }

    private void OnPlayerSpawned(Transform playerTransform)
    {
        PlayerHealth health = playerTransform.GetComponent<PlayerHealth>();
        PlayerController controller = playerTransform.GetComponent<PlayerController>();
        if (health == null || controller == null) return;

        ulong clientId = controller.OwnerClientId;

        // Evitar registrar el mismo jugador dos veces
        if (_clientHearts.ContainsKey(clientId)) return;

        Image[] hearts = AssignPanel(clientId, controller.IsOwner);
        if (hearts == null) return;

        _clientHearts[clientId] = hearts;
        _clientHealth[clientId] = health;

        health.OnHealthChanged += (current, max) => UpdateHearts(clientId, current, max);
        UpdateHearts(clientId, 3, 3);
    }

    private Image[] AssignPanel(ulong clientId, bool isLocal)
    {
        if (isLocal)
            return _localHearts;

        if (!_player2Panel.activeSelf)
        {
            _player2Panel.SetActive(true);
            return _player2Hearts;
        }
        if (!_player3Panel.activeSelf)
        {
            _player3Panel.SetActive(true);
            return _player3Hearts;
        }
        if (!_player4Panel.activeSelf)
        {
            _player4Panel.SetActive(true);
            return _player4Hearts;
        }

        return null;
    }

    private void UpdateHearts(ulong clientId, int current, int max)
    {
        if (!_clientHearts.ContainsKey(clientId)) return;

        Image[] hearts = _clientHearts[clientId];
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null) continue;
            hearts[i].sprite = i < current ? _heartFull : _heartEmpty;
        }
    }

    private void OnDestroy()
    {
        Main.CustomEvents.OnAnyPlayerSpawned.RemoveListener(OnPlayerSpawned);
    }
}