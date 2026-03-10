using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class RespawnManager : NetworkBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [SerializeField] private float _respawnDelay = 3f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log($"<color=cyan>RespawnManager spawned. IsServer={IsServer}</color>");
    }

    public void RequestRespawn(ulong clientId, Vector3 deathPosition, bool killedByPlayer)
    {
        if (!IsServer) return;
        StartCoroutine(RespawnRoutine(clientId, deathPosition, killedByPlayer));
    }

    private IEnumerator RespawnRoutine(ulong clientId, Vector3 deathPosition, bool killedByPlayer)
    {
        yield return new WaitForSeconds(_respawnDelay);

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)) yield break;

        var playerNetObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerNetObj == null) yield break;

        Vector3 respawnPos;
        if (killedByPlayer)
        {
            respawnPos = DungeonGenerator.Instance != null
                ? DungeonGenerator.Instance.GetRandomRespawnPosition(deathPosition)
                : new Vector3(0, 1, 0);
        }
        else
        {
            respawnPos = new Vector3(deathPosition.x, 1f, deathPosition.z);
        }

        // Solo el servidor reposiciona y resetea
        playerNetObj.transform.position = respawnPos;
        PlayerHealth health = playerNetObj.GetComponent<PlayerHealth>();
        if (health != null)
            health.ResetHealth(); // esto dispara OnDeadChanged en todos los clientes

        // Notificamos solo para cámara e iframes — NO reposicionamos aquí
        RespawnClientRpc(clientId, respawnPos);
    }

    [ClientRpc]
    private void RespawnClientRpc(ulong clientId, Vector3 position)
    {
        // Buscamos solo entre objetos activos — el jugador nunca se desactiva
        PlayerController[] allPlayers = FindObjectsByType<PlayerController>(
            FindObjectsSortMode.None);

        foreach (var p in allPlayers)
        {
            if (p.OwnerClientId != clientId) continue;

            // Reposicionamos en el cliente también para sincronizar
            p.transform.position = position;

            // iframes al respawnear solo al dueño
            PlayerHealth health = p.GetComponent<PlayerHealth>();
            if (health != null)
                health.TriggerRespawnIFrames();

            // Cámara solo si es el jugador local
            if (p.IsOwner)
                Main.CustomEvents.OnLocalPlayerSpawned?.Invoke(p.transform);

            break;
        }
    }
}