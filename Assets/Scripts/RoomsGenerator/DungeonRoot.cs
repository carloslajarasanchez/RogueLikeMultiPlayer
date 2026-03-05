using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonRoot : MonoBehaviour
{
    private RoomTemplates _templates;

    void Awake()
    {
        _templates = GameObject.FindGameObjectWithTag("Rooms").GetComponent<RoomTemplates>();
    }

    void Start()
    {
        // 1. Elegimos una lista al azar (Arriba, Abajo, Izquierda o Derecha)
        int randomList = Random.Range(0, 4);
        GameObject[] listaElegida;

        switch (randomList)
        {
            case 0: listaElegida = _templates.BottomRooms; break;
            case 1: listaElegida = _templates.TopRooms; break;
            case 2: listaElegida = _templates.LeftRooms; break;
            default: listaElegida = _templates.RightRooms; break;
        }

        // 2. Instanciamos una sala aleatoria de esa lista en el centro
        int rand = Random.Range(0, listaElegida.Length);
        Instantiate(listaElegida[rand], Vector3.zero, Quaternion.identity);

        // El propio prefab instanciado ya trae sus "RoomSpawners", 
        // por lo que la mazmorra crecerá sola desde ahí.
    }
}
