using UnityEngine;
using UnityEngine.Events;

public class CustomEvents
{
    // Para la cámara — solo el jugador local
    public UnityEvent<Transform> OnLocalPlayerSpawned = new UnityEvent<Transform>();
    // Para la UI — todos los jugadores
    public UnityEvent<Transform> OnAnyPlayerSpawned = new UnityEvent<Transform>();
}