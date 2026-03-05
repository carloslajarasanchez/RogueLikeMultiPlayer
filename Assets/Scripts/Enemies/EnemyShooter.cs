using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooter : Enemy
{
    [Header("Ajustes de Tirador")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRate = 2f;
    public float speed = 4f;

    private Vector3 targetPosition;
    private bool isMoving = false;

    [SerializeField] private int poolSize = 5;
    private List<GameObject> myPool = new List<GameObject>();

    private float _updateTargetInterval = 1f;
    private float _nextUpdateTime = 0f;

    protected override void Start()
    {
        base.Start();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject b = Instantiate(bulletPrefab);
            b.SetActive(false);
            myPool.Add(b);
        }

        StartCoroutine(ShootingRoutine());
    }

    IEnumerator ShootingRoutine()
    {
        yield return new WaitForSeconds(Random.Range(0f, 2f));

        while (true)
        {
            if (player != null)
                Shoot();

            yield return new WaitForSeconds(1f);

            targetPosition = transform.position + new Vector3(
                Random.Range(-5, 5), 0, Random.Range(-5, 5));
            isMoving = true;

            yield return new WaitForSeconds(fireRate);
            isMoving = false;
            rb.velocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (Time.time >= _nextUpdateTime)
        {
            FindClosestPlayer();
            _nextUpdateTime = Time.time + _updateTargetInterval;
        }

        if (player != null)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;
            rb.MoveRotation(Quaternion.LookRotation(dir));
        }

        if (isMoving)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0;
            Vector3 newPos = transform.position + direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);

            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
                isMoving = false;
        }
    }

    void Shoot()
    {
        foreach (GameObject b in myPool)
        {
            if (!b.activeInHierarchy)
            {
                b.transform.position = firePoint.position;
                b.transform.rotation = firePoint.rotation;
                b.SetActive(true);

                Projectile projectile = b.GetComponent<Projectile>();
                if (projectile != null)
                    projectile.Launch(firePoint.forward);

                return;
            }
        }
    }
}