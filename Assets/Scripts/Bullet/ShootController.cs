using System.Collections.Generic;
using UnityEngine;

public class ShootController : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Configuración de Disparo")]
    public float fireRate = 0.25f;
    private float nextFireTime = 0f;

    [Header("Pool de Balas")]
    [SerializeField] private int poolSize = 15;
    private List<GameObject> bulletPool;

    void Start()
    {
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
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }
    }

    private void Shoot()
    {
        GameObject bullet = GetPooledBullet();
        if (bullet == null) return;

        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        bullet.SetActive(true);

        // Usamos Launch en lugar de AddForce directo
        Projectile projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
            projectile.Launch(firePoint.forward);
    }

    private GameObject GetPooledBullet()
    {
        foreach (GameObject bullet in bulletPool)
            if (!bullet.activeInHierarchy) return bullet;
        return null;
    }
}