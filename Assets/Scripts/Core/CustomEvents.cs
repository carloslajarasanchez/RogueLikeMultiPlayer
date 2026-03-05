using UnityEngine;
using UnityEngine.Events;

public class CustomEvents
{
    public UnityEvent<Transform> OnPlayerSpawned = new UnityEvent<Transform>();
}
