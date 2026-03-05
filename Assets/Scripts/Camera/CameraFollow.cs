using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Cam Settings")]
    [SerializeField] private float _lerpSpeed = 5f;    // Qué tan suave es el seguimiento
    [SerializeField] private float _heightOffset = 5f; // El offset constante en el eje Y
    [SerializeField] private float _zOffset = -5f; // El offset constante en el eje Y

    private Transform _target;        // El transform de tu jugador

    private void Awake()
    {
        Main.CustomEvents.OnPlayerSpawned?.AddListener(SetTarget);
    }

    void FixedUpdate()
    {
        if (_target == null) return;

        // 1. Creamos el vector de destino
        // Mantenemos X y Z del jugador, pero fijamos la Y con nuestro offset
        Vector3 targetPosition = new Vector3(_target.position.x, _heightOffset, _target.position.z + _zOffset);

        // 2. Aplicamos una interpolación lineal (Lerp) para que el movimiento sea fluido
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, _lerpSpeed * Time.deltaTime);

        // 3. Aplicamos la posición final a la cámara
        transform.position = smoothedPosition;
    }

    private void SetTarget(Transform playerTransform)
    {
        _target = playerTransform; // no necesitamos buscar nada

        transform.position = new Vector3(
        _target.position.x,
        _heightOffset,
        _target.position.z + _zOffset
    );
    }
}
