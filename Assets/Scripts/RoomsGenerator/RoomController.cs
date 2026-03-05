using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("Configuración")]
    public GameObject[] enemyPrefabs;
    public Transform[] spawnPoints; // Puntos dentro de la sala donde salen enemigos
    public List<DoorsMove> doorsInRoom; // Arrastra aquí las puertas de este prefab

    private List<GameObject> _enemiesAlive = new List<GameObject>();
    private bool _roomActive = false;
    private bool _roomCleared = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !_roomActive && !_roomCleared)
        {
            StartCoroutine(StartRoomSequence());
        }
    }

    IEnumerator StartRoomSequence()
    {
        _roomActive = true;
        foreach (var door in doorsInRoom) if (door != null) door.CloseDoor();

        yield return new WaitForSeconds(0.5f);

        // 1. Creamos una copia de la lista de spawnPoints para no romper la original
        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        int amount = Random.Range(2, 6);

        // Si queremos spawnear más enemigos que puntos hay, limitamos el amount
        amount = Mathf.Min(amount, availablePoints.Count);

        for (int i = 0; i < amount; i++)
        {
            // 2. Elegimos un índice al azar de los puntos que QUEDAN
            int randomIndex = Random.Range(0, availablePoints.Count);
            Transform sp = availablePoints[randomIndex];

            // 3. Quitamos el punto de la lista para que no se use otra vez
            availablePoints.RemoveAt(randomIndex);

            // 4. Instanciamos
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

    void OnEnemyDefeated(GameObject enemy)
    {
        // 1. Intentamos eliminarlo de la lista
        if (_enemiesAlive.Contains(enemy))
        {
            _enemiesAlive.Remove(enemy);
        }
        else
        {
            // Esto nos dirá si estamos intentando matar a un enemigo que no estaba en la lista original
            Debug.LogWarning("Se intentó eliminar a " + enemy.name + " pero no estaba en la lista de la sala.");
        }

        Debug.Log("<color=orange>Enemigos restantes: </color>" + _enemiesAlive.Count);

        // 2. Comprobación de seguridad: 
        // Usamos <= 0 por si acaso y verificamos que el objeto no sea nulo
        if (_enemiesAlive.Count <= 0)
        {
            Debug.Log("Contador llegó a cero. Llamando a ClearRoom...");
            ClearRoom();
        }
    }

    void ClearRoom()
    {
        _roomCleared = true;
        _roomActive = false;
        Debug.Log("Abriendo puertas...");
        // 3. Abrir puertas al limpiar la sala
        foreach (var door in doorsInRoom) door.OpenDoor();
    }
}
