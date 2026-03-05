using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkManagerUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private GameObject _lobbyPanel;

    [Header("Menu Referencias")]
    [SerializeField] private TMP_InputField _joinCodeInput;
    [SerializeField] private TextMeshProUGUI _errorText;

    [Header("Lobby Referencias")]
    [SerializeField] private TextMeshProUGUI _lobbyCodeText;
    [SerializeField] private TextMeshProUGUI _playersCountText;
    [SerializeField] private Button _startGameButton; // solo visible para el host

    [Header("Configuración")]
    [SerializeField] private string _gameSceneName = "GameScene";
    [SerializeField] private int _maxPlayers = 4;

    private UnityTransport _transport;
    private string _currentCode;

    private void Start()
    {
        DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
        _transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        _menuPanel.SetActive(true);
        _lobbyPanel.SetActive(false);
        _errorText.text = "";

        // Suscribirse a eventos de red
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton == null) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
    }

    // ── Botón "Crear Sala" ──
    public void OnCreateRoom()
    {
        _currentCode = GenerateRoomCode();

        // Configuramos el puerto usando el código como semilla
        // En local ambos usarán el mismo puerto
        _transport.SetConnectionData("127.0.0.1", 7777);

        NetworkManager.Singleton.StartHost();

        _menuPanel.SetActive(false);
        _lobbyPanel.SetActive(true);
        _lobbyCodeText.text = $"Código de sala: {_currentCode}";
        _startGameButton.gameObject.SetActive(true); // solo el host ve el botón
        UpdatePlayerCount();
    }

    // ── Botón "Unirse a Sala" ──
    public void OnJoinRoom()
    {
        string code = _joinCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(code))
        {
            _errorText.text = "Introduce un código de sala.";
            return;
        }

        _transport.SetConnectionData("127.0.0.1", 7777);
        NetworkManager.Singleton.StartClient();

        _menuPanel.SetActive(false);
        _lobbyPanel.SetActive(true);
        _lobbyCodeText.text = $"Uniéndose a: {code}";
        _startGameButton.gameObject.SetActive(false); // clientes no ven el botón
    }

    // ── Botón "Iniciar Partida" (solo host) ──
    public void OnStartGame()
    {
        if (!NetworkManager.Singleton.IsHost) return;
        if (NetworkManager.Singleton.ConnectedClients.Count < 1) return;

        NetworkManager.Singleton.SceneManager.LoadScene(
            _gameSceneName, LoadSceneMode.Single);
    }

    // ── Botón "Volver" ──
    public void OnBack()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
            NetworkManager.Singleton.Shutdown();

        _menuPanel.SetActive(true);
        _lobbyPanel.SetActive(false);
        _errorText.text = "";
    }

    private void OnClientConnected(ulong clientId)
    {
        UpdatePlayerCount();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        // Si nos desconectan volvemos al menú
        if (!NetworkManager.Singleton.IsHost)
        {
            _menuPanel.SetActive(true);
            _lobbyPanel.SetActive(false);
            _errorText.text = "Desconectado de la sala.";
        }
        UpdatePlayerCount();
    }

    private void UpdatePlayerCount()
    {
        if (NetworkManager.Singleton == null) return;

        // Solo el host puede acceder a ConnectedClients
        if (!NetworkManager.Singleton.IsServer) return;

        int count = NetworkManager.Singleton.ConnectedClients.Count;
        _playersCountText.text = $"Jugadores: {count}/{_maxPlayers}";
    }

    private string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] code = new char[6];
        for (int i = 0; i < 6; i++)
            code[i] = chars[Random.Range(0, chars.Length)];
        return new string(code);
    }
}