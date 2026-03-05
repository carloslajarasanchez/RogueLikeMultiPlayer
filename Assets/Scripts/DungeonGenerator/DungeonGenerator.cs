using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DungeonGenerator : NetworkBehaviour
{
    [Header("Configuración del Mapa")]
    [SerializeField] private int _minRooms = 8;
    [SerializeField] private int _maxRooms = 15;
    [SerializeField] private int _maxAttempts = 50;

    [Header("Tamaño de Sala + Pasillo")]
    [SerializeField] private float _roomWidth = 10f;
    [SerializeField] private float _roomHeight = 10f;
    [SerializeField] private float _corridorLength = 5f;

    [Header("Offsets de Pasillos")]
    [SerializeField] private float _corridorOffsetX = 3.5f;
    [SerializeField] private float _corridorOffsetZ = -1.5f;
    [SerializeField] private float _corridorPivotCorrection = -2f;

    [Header("Assets")]
    [SerializeField] private RoomPrefabLibrary _library;
    [SerializeField] private GameObject _corridorH;
    [SerializeField] private GameObject _corridorV;

    [Header("Player")]
    [SerializeField] private GameObject _playerPrefab;

    private Dictionary<Vector2Int, RoomData> _map = new Dictionary<Vector2Int, RoomData>();
    private List<Vector3> _spawnPositions = new List<Vector3>();

    private static readonly Vector2Int DirTop = Vector2Int.up;
    private static readonly Vector2Int DirBottom = Vector2Int.down;
    private static readonly Vector2Int DirLeft = Vector2Int.left;
    private static readonly Vector2Int DirRight = Vector2Int.right;

    public override void OnNetworkSpawn()
    {
        if (!IsHost) return;

        _library.Initialize();

        int attempts = 0;
        do
        {
            ClearMap();
            GenerateMap();
            attempts++;
        }
        while (_map.Count < _minRooms && attempts < _maxAttempts);

        if (_map.Count < _minRooms)
            Debug.LogWarning($"Mínimo no alcanzado tras {_maxAttempts} intentos. Salas: {_map.Count}");

        AssignRoomTypes();
        AssignDoors();
        InstantiateMap();
        SpawnPlayers();

        Debug.Log($"<color=cyan>Generación completa: {_map.Count} salas.</color>");
    }

    // ──────────────────────────────────────────────────────
    // PASO 1 — Random Walk
    // ──────────────────────────────────────────────────────
    private void GenerateMap()
    {
        int target = Random.Range(_minRooms, _maxRooms + 1);
        Queue<Vector2Int> frontier = new Queue<Vector2Int>();

        _map[Vector2Int.zero] = new RoomData(0, 0);
        frontier.Enqueue(Vector2Int.zero);

        while (_map.Count < target && frontier.Count > 0)
        {
            Vector2Int current = frontier.Dequeue();

            List<Vector2Int> dirs = new List<Vector2Int>
                { DirTop, DirBottom, DirLeft, DirRight };
            Shuffle(dirs);

            foreach (Vector2Int dir in dirs)
            {
                if (_map.Count >= target) break;

                Vector2Int next = current + dir;
                if (_map.ContainsKey(next)) continue;

                float branchChance = _map.Count < _minRooms ? 0.85f : 0.55f;
                if (Random.value > branchChance) continue;

                _map[next] = new RoomData(next.x, next.y);
                frontier.Enqueue(next);
            }
        }
    }

    // ──────────────────────────────────────────────────────
    // PASO 2 — Sala inicio y sala final
    // ──────────────────────────────────────────────────────
    private void AssignRoomTypes()
    {
        _map[Vector2Int.zero].Type = RoomType.Start;

        Vector2Int farthestKey = Vector2Int.zero;
        int maxDist = 0;

        foreach (var key in _map.Keys)
        {
            int dist = Mathf.Abs(key.x) + Mathf.Abs(key.y);
            if (dist > maxDist)
            {
                maxDist = dist;
                farthestKey = key;
            }
        }

        if (farthestKey != Vector2Int.zero)
            _map[farthestKey].Type = RoomType.End;
    }

    // ──────────────────────────────────────────────────────
    // PASO 3 — Calcular puertas
    // ──────────────────────────────────────────────────────
    private void AssignDoors()
    {
        foreach (var kvp in _map)
        {
            Vector2Int pos = kvp.Key;
            RoomData room = kvp.Value;

            room.DoorTop = _map.ContainsKey(pos + DirTop);
            room.DoorBottom = _map.ContainsKey(pos + DirBottom);
            room.DoorLeft = _map.ContainsKey(pos + DirLeft);
            room.DoorRight = _map.ContainsKey(pos + DirRight);
        }
    }

    // ──────────────────────────────────────────────────────
    // PASO 4 — Instanciar prefabs
    // ──────────────────────────────────────────────────────
    private void InstantiateMap()
    {
        RoomTemplates templates = FindFirstObjectByType<RoomTemplates>();
        float stepX = _roomWidth + _corridorLength;
        float stepZ = _roomHeight + _corridorLength;

        _spawnPositions.Clear();

        foreach (var kvp in _map)
        {
            Vector2Int gridPos = kvp.Key;
            RoomData room = kvp.Value;

            Vector3 worldPos = new Vector3(
                gridPos.x * stepX,
                0f,
                gridPos.y * stepZ
            );

            _spawnPositions.Add(worldPos);

            string key = room.GetPrefabKey();
            GameObject prefab = _library.GetPrefab(key);
            if (prefab == null) continue;

            // Host instancia localmente
            GameObject go = Instantiate(prefab, worldPos, Quaternion.identity);
            go.name = $"Room_{gridPos.x}_{gridPos.y}_{key}";
            if (room.Type == RoomType.Start) go.name += "_START";
            if (room.Type == RoomType.End) go.name += "_END";
            templates?.Rooms.Add(go);

            // Clientes instancian la misma sala
            SpawnRoomClientRpc(key, worldPos,
                room.Type == RoomType.Start,
                room.Type == RoomType.End);

            // — Pasillo Norte-Sur (Top) —
            if (room.DoorTop && _corridorV != null)
            {
                Vector3 corridorPos = worldPos + new Vector3(
                    _corridorOffsetX,
                    0.5f,
                    _roomHeight / 2f + _corridorLength / 2f + _corridorPivotCorrection
                );
                Instantiate(_corridorV, corridorPos, Quaternion.identity);
                SpawnCorridorClientRpc(corridorPos, true);
            }

            // — Pasillo Este-Oeste (Right) —
            if (room.DoorRight && _corridorH != null)
            {
                Vector3 corridorPos = worldPos + new Vector3(
                    _roomWidth / 2f + _corridorLength / 2f + _corridorOffsetX - 0.5f,
                    0.5f,
                    _corridorOffsetZ
                );
                Instantiate(_corridorH, corridorPos, Quaternion.identity);
                SpawnCorridorClientRpc(corridorPos, false);
            }
        }
    }

    [ClientRpc]
    private void SpawnRoomClientRpc(string key, Vector3 position, bool isStart, bool isEnd)
    {
        if (IsHost) return;

        _library.Initialize();
        RoomTemplates templates = FindFirstObjectByType<RoomTemplates>();
        GameObject prefab = _library.GetPrefab(key);
        if (prefab == null) return;

        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        go.name = $"Room_{key}";
        if (isStart) go.name += "_START";
        if (isEnd) go.name += "_END";
        templates?.Rooms.Add(go);
    }

    [ClientRpc]
    private void SpawnCorridorClientRpc(Vector3 position, bool isVertical)
    {
        if (IsHost) return;

        GameObject prefab = isVertical ? _corridorV : _corridorH;
        if (prefab == null) return;
        Instantiate(prefab, position, Quaternion.identity);
    }

    // ──────────────────────────────────────────────────────
    // PASO 5 — Spawn de jugadores
    // ──────────────────────────────────────────────────────
    private void SpawnPlayers()
    {
        List<Vector3> playerSpawns = GetSpreadSpawnPoints(
            NetworkManager.Singleton.ConnectedClients.Count);

        int index = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            Vector3 spawnPos = index < playerSpawns.Count
                ? playerSpawns[index] + Vector3.up
                : Vector3.up;

            GameObject player = Instantiate(_playerPrefab, spawnPos, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.Key);

            index++;
        }
    }

    private List<Vector3> GetSpreadSpawnPoints(int count)
    {
        List<Vector3> sorted = new List<Vector3>(_spawnPositions);
        sorted.Sort((a, b) =>
            Vector3.Distance(b, Vector3.zero).CompareTo(Vector3.Distance(a, Vector3.zero)));

        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < Mathf.Min(count, sorted.Count); i++)
            result.Add(sorted[i]);

        return result;
    }

    // ──────────────────────────────────────────────────────
    // Utilidades
    // ──────────────────────────────────────────────────────
    private void ClearMap()
    {
        _map.Clear();
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void OnDrawGizmos()
    {
        if (_map == null || _map.Count == 0) return;

        float stepX = _roomWidth + _corridorLength;
        float stepZ = _roomHeight + _corridorLength;

        foreach (var kvp in _map)
        {
            Vector3 pos = new Vector3(
                kvp.Key.x * stepX,
                0.5f,
                kvp.Key.y * stepZ
            );

            Gizmos.color = kvp.Value.Type == RoomType.Start ? Color.green
                         : kvp.Value.Type == RoomType.End ? Color.red
                         : Color.white;

            Gizmos.DrawWireCube(pos,
                new Vector3(_roomWidth * 0.9f, 1f, _roomHeight * 0.9f));
        }
    }
}