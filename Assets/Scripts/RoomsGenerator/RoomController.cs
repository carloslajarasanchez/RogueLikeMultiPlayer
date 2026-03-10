using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public List<DoorTrigger> doorsInRoom;
    [SerializeField] private float _doorCloseDelay = 0.5f;

    private List<GameObject> _enemiesAlive = new List<GameObject>();
    private bool _roomActive = false;
    private bool _roomCleared = false;

    private void Start()
    {
        foreach (var door in doorsInRoom)
        {
            if (door == null) continue;
            door.Init(this);

            // Registrar en DungeonGenerator para sincronización en red
            if (DungeonGenerator.Instance != null)
                DungeonGenerator.Instance.RegisterDoor(door);
        }
    }

    public void TryActivateRoom()
    {
        if (_roomCleared) return;
        if (_roomActive) return;
        StartCoroutine(StartRoomSequence());
    }

    IEnumerator StartRoomSequence()
    {
        _roomActive = true;

        yield return new WaitForSeconds(_doorCloseDelay);

        foreach (var door in doorsInRoom)
        {
            if (door == null) continue;
            door.CloseDoor(); // local (servidor)
            DungeonGenerator.Instance?.SyncCloseDoor(door.NetworkDoorId); // clientes
        }

        yield return new WaitForSeconds(0.5f);

        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        int amount = Mathf.Min(Random.Range(2, 6), availablePoints.Count);

        List<Transform> selectedPoints = new List<Transform>();
        for (int i = 0; i < amount; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            selectedPoints.Add(availablePoints[randomIndex]);
            availablePoints.RemoveAt(randomIndex);
        }

        if (DungeonGenerator.Instance != null)
            DungeonGenerator.Instance.SpawnEnemiesForRoom(
                enemyPrefabs, selectedPoints, OnEnemySpawned);
    }

    private void OnEnemySpawned(GameObject enemy)
    {
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.OnDeath += OnEnemyDefeated;
            _enemiesAlive.Add(enemy);
        }
    }

    public void RequestDoorOpen(DoorTrigger door)
    {
        // Solo el servidor gestiona la apertura de puertas
        if (Unity.Netcode.NetworkManager.Singleton != null &&
            !Unity.Netcode.NetworkManager.Singleton.IsServer) return;

        if (_roomActive) return;
        bool enteringRoom = !_roomCleared;
        door.OpenDoor(enteringRoom);
        DungeonGenerator.Instance?.SyncOpenDoor(door.NetworkDoorId, enteringRoom);
    }

    void OnEnemyDefeated(GameObject enemy)
    {
        if (_enemiesAlive.Contains(enemy))
            _enemiesAlive.Remove(enemy);

        Debug.Log($"<color=orange>Enemigos restantes: {_enemiesAlive.Count}</color>");

        if (_enemiesAlive.Count <= 0)
            ClearRoom();
    }

    void ClearRoom()
    {
        _roomCleared = true;
        _roomActive = false;
        Debug.Log("<color=green>Sala completada.</color>");
        // No abrir puertas aquí — se abren cuando el jugador se acerca
    }
}