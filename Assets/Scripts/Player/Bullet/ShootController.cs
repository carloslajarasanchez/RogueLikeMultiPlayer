using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Configuración de Disparo")]
    public float bulletForce = 20f;
    public float fireRate = 0.25f; // Tiempo entre disparos
    private float nextFireTime = 0f;

    [Header("Pool de Balas")]
    [SerializeField] private int poolSize = 15;
    private List<GameObject> bulletPool;

    void Start()
    {
        // Inicializamos el pool solo para este jugador
        bulletPool = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(bulletPrefab);
            obj.SetActive(false);
            bulletPool.Add(obj);
        }
    }

    void Update()
    {
        // Disparar con click izquierdo y respetando el cooldown
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        GameObject bullet = GetPooledBullet();

        if (bullet != null)
        {
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;
            bullet.SetActive(true);

            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Resetear velocidad para que no herede la del disparo anterior
                rb.velocity = Vector3.zero;
                rb.AddForce(firePoint.forward * bulletForce, ForceMode.Impulse);
            }
        }
    }

    private GameObject GetPooledBullet()
    {
        // Busca una bala desactivada en la lista privada
        foreach (GameObject bullet in bulletPool)
        {
            if (!bullet.activeInHierarchy)
            {
                return bullet;
            }
        }
        return null; // Si están todas usadas, no dispara
    }
}
