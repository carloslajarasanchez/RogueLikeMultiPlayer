using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomPrefabLibrary", menuName = "Dungeon/Room Prefab Library")]
public class RoomPrefabLibrary : ScriptableObject
{
    [System.Serializable]
    public struct RoomEntry
    {
        public string key;        // "T", "TB", "TBL", "CentralR", etc.
        public GameObject prefab;
    }

    public List<RoomEntry> entries = new List<RoomEntry>();

    private Dictionary<string, GameObject> _dict;

    // Llama esto antes de usarla
    public void Initialize()
    {
        _dict = new Dictionary<string, GameObject>();
        foreach (var e in entries)
            _dict[e.key] = e.prefab;
    }

    public GameObject GetPrefab(string key)
    {
        if (_dict == null) Initialize();
        if (_dict.TryGetValue(key, out GameObject prefab)) return prefab;

        // Fallback: si no existe la combinación exacta, usamos ClosedR
        Debug.LogWarning($"No se encontró prefab para la clave '{key}', usando ClosedR.");
        return _dict.TryGetValue("ClosedR", out GameObject fallback) ? fallback : null;
    }
}