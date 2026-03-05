using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomTemplates : MonoBehaviour
{
    public GameObject[] BottomRooms;
    public GameObject[] TopRooms;
    public GameObject[] LeftRooms;
    public GameObject[] RightRooms;

    public GameObject ClosedRoom;

    public List<GameObject> Rooms;// Refenrencia de las salas creadas

    public GameObject Boss;
    public GameObject Enemies;

    private void Start()
    {
       // Invoke("SpawnEnemies", 3f);
    }

    private void SpawnEnemies()
    {
        //Creacion de Boss
        Instantiate(Boss, Rooms[Rooms.Count - 1].transform.position, Quaternion.identity);

        //Creacion de enemigos
        for(int i = 0; i < Rooms.Count - 1; i++)
        {
            Instantiate(Enemies, Rooms[i].transform.position, Quaternion.identity);
        }
    }
}
