using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorsSpawnController : MonoBehaviour
{
    // Usamos una variable estática para dar un ID único a cada puerta
    // Esto nos servirá para decidir cuál se queda y cuál se va
    private int _doorID;
    private static int _idCounter = 0;

    private void Start()
    {
        _doorID = _idCounter++;
    }

    private IEnumerator OnTriggerEnter(Collider other)
    {

        DoorsSpawnController otherDoor = other.GetComponent<DoorsSpawnController>();

        if (otherDoor != null)
        {
            yield return new WaitForSeconds(0.1f);
            if (this._doorID > otherDoor._doorID)
            {
                Debug.Log("Puerta duplicada detectada. Eliminando redundancia.");
                Destroy(gameObject);
            }
        }
    }

    /*private IEnumerator OnTriggerStay(Collider other)
    {
        // Si el otro objeto es otra puerta, esperamos un momento para asegurarnos de que ambas puertas hayan asignado su ID
        DoorsSpawnController otherDoor = other.GetComponent<DoorsSpawnController>();
        if (otherDoor != null)
        {
            yield return new WaitForSeconds(0.1f); // Esperamos un poco
            if (this._doorID > otherDoor._doorID)
            {
                Debug.Log("Puerta duplicada detectada en OnTriggerStay. Eliminando redundancia.");
                Destroy(gameObject);
            }
        }
    }*/
}

