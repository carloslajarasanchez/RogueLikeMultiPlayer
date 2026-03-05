using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody _rigidbody;
    private Vector3 _moveInput;
    private Camera _mainCam;

    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();

        // Si no asignas la cámara en el inspector, la busca automáticamente
        if (_mainCam == null) _mainCam = Camera.main;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // Entrada de movimiento (X, Z)
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.z = Input.GetAxisRaw("Vertical");

        // Lógica de rotación hacia el ratón (Raycast)
        Ray ray = _mainCam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // Un plano invisible en el suelo
        float rayDistance;

        if (groundPlane.Raycast(ray, out rayDistance))
        {
            Vector3 pointToLook = ray.GetPoint(rayDistance);
            // Mirar al punto del ratón pero manteniendo la altura del personaje
            transform.LookAt(new Vector3(pointToLook.x, transform.position.y, pointToLook.z));
        }
    }

    void FixedUpdate()
    {
        // Movimiento físico
        _rigidbody.MovePosition(_rigidbody.position + _moveInput.normalized * _moveSpeed * Time.fixedDeltaTime);
    }
}
