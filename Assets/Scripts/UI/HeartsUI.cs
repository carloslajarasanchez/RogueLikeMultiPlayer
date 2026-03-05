using UnityEngine;
using UnityEngine.UI;

public class HeartsUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Sprite _heartFull;
    [SerializeField] private Sprite _heartEmpty;
    [SerializeField] private Image[] _heartImages; // arrastra los 3 Image de la UI aquí

    private PlayerHealth _playerHealth;

    private void Awake()
    {
        Main.CustomEvents.OnPlayerSpawned.AddListener(OnPlayerSpawned);
    }

    private void OnPlayerSpawned(Transform playerTransform)
    {
        _playerHealth = playerTransform.GetComponent<PlayerHealth>();
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged += UpdateHearts;

        // Inicializamos la UI con los corazones llenos
        UpdateHearts(_heartImages.Length, _heartImages.Length);
    }

    private void UpdateHearts(int current, int max)
    {
        for (int i = 0; i < _heartImages.Length; i++)
        {
            if (i < current)
                _heartImages[i].sprite = _heartFull;
            else
                _heartImages[i].sprite = _heartEmpty;
        }
    }

    private void OnDestroy()
    {
        if (_playerHealth != null)
            _playerHealth.OnHealthChanged -= UpdateHearts;
    }
}