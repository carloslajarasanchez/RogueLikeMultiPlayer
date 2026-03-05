using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ShootController : NetworkBehaviour
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

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        InitPool();
    }

    private void InitPool()
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
        if (!IsOwner) return;

        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            ShootServerRpc(firePoint.position, firePoint.rotation, firePoint.forward);
            nextFireTime = Time.time + fireRate;
        }
    }

    [ServerRpc]
    private void ShootServerRpc(Vector3 position, Quaternion rotation, Vector3 direction)
    {
        GameObject bullet = Instantiate(bulletPrefab, position, rotation);
        bullet.GetComponent<NetworkObject>().Spawn();

        Projectile projectile = bullet.GetComponent<Projectile>();
        if (projectile != null)
        {
            Collider ownerCollider = GetComponent<Collider>();
            projectile.Launch(direction, ownerCollider);
        }
    }

    private GameObject GetPooledBullet()
    {
        foreach (GameObject bullet in bulletPool)
        {
            if (bullet == null) continue;
            if (!bullet.activeInHierarchy) return bullet;
        }
        return null;
    }
}