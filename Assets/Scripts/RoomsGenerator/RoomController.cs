using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints;
    public List<DoorTrigger> doorsInRoom; // <- ahora es DoorTrigger

    [SerializeField] private float _doorCloseDelay = 0.5f;

    private List<GameObject> _enemiesAlive = new List<GameObject>();
    private bool _roomActive = false;
    private bool _roomCleared = false;

    private void Start()
    {
        foreach (var door in doorsInRoom)
            if (door != null) door.Init(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (_roomCleared) return;
        if (_roomActive) return;

        StartCoroutine(StartRoomSequence());
    }

    IEnumerator StartRoomSequence()
    {
        _roomActive = true;

        yield return new WaitForSeconds(_doorCloseDelay);

        foreach (var door in doorsInRoom)
            if (door != null) door.CloseDoor();

        yield return new WaitForSeconds(0.5f);

        List<Transform> availablePoints = new List<Transform>(spawnPoints);
        int amount = Mathf.Min(Random.Range(2, 6), availablePoints.Count);

        for (int i = 0; i < amount; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform sp = availablePoints[randomIndex];
            availablePoints.RemoveAt(randomIndex);

            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(prefab, sp.position, Quaternion.identity);

            Enemy health = enemy.GetComponent<Enemy>();
            if (health != null)
            {
                health.OnDeath += OnEnemyDefeated;
                _enemiesAlive.Add(enemy);
            }
        }
    }

    public void RequestDoorOpen(DoorTrigger door)
    {
        if (_roomActive) return;
        bool enteringRoom = !_roomCleared;
        door.OpenDoor(enteringRoom);
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
    }
}
