using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorsMove : MonoBehaviour
{
    [SerializeField] private float _speedRotation = 0.2f; // Qué tan rápido gira
    [SerializeField] private float _grades = 90f; // Qué tan rápido gira

    private bool _isRotating = false;
    private bool _isOpen = true; // Empiezan abiertas para que el jugador entre
    private Quaternion _initialRotation;
    private Quaternion _closedRotation;

    void Awake()
    {
        // Guardamos la rotación inicial (abierta) y calculamos la cerrada
        _initialRotation = transform.rotation;
        _closedRotation = _initialRotation * Quaternion.Euler(0, _grades, 0);
    }

    public void CloseDoor()
    {
        if (_isOpen && !_isRotating)
        {
            StartCoroutine(SmoothRotate(_initialRotation));
            _isOpen = false;
        }
    }

    public void OpenDoor()
    {
        Debug.Log("Intentando abrir puerta...");
        if (!_isOpen && !_isRotating)
        {
            Debug.Log("Abiertas...");
            StartCoroutine(SmoothRotate(_closedRotation));
            _isOpen = true;
        }
    }

    IEnumerator SmoothRotate(Quaternion targetRotation)
    {
        _isRotating = true;
        Quaternion startRot = transform.rotation;
        float elapsed = 0;

        while (elapsed < _speedRotation)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRotation, elapsed / _speedRotation);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        _isRotating = false;
    }
}
