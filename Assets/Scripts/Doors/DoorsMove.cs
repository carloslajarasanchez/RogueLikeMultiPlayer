using System.Collections;
using UnityEngine;

public class DoorsMove : MonoBehaviour
{
    [SerializeField] private float _speedRotation = 0.2f;
    [SerializeField] private float _grades = 90f;

    private bool _isRotating = false;
    private bool _isOpen = false;
    private Quaternion _closedRotation;  // rotación inicial del prefab = cerrada
    private Quaternion _openRotation;    // rotación girada 90° = abierta

    void Awake()
    {
        _closedRotation = transform.rotation;
        _openRotation = _closedRotation * Quaternion.Euler(0, _grades, 0);

        // Empiezan cerradas
        _isOpen = false;
        transform.rotation = _closedRotation;
    }

    public void OpenOutward()
    {
        if (!_isOpen && !_isRotating)
        {
            StartCoroutine(SmoothRotate(_openRotation));
            _isOpen = true;
        }
    }
    public void OpenInward()
    {
        if (!_isOpen && !_isRotating)
        {
            Quaternion inwardRotation = _closedRotation * Quaternion.Euler(0, -_grades, 0);
            StartCoroutine(SmoothRotate(inwardRotation));
            _isOpen = true;
        }
    }

    public void CloseDoor()
    {
        if (_isOpen && !_isRotating)
        {
            StartCoroutine(SmoothRotate(_closedRotation));
            _isOpen = false;
        }
    }

    public bool IsOpen => _isOpen;

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