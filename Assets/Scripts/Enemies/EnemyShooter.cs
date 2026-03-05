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

    // Variables de pooling (reutilizando tu lógica anterior)
    [SerializeField] private int poolSize = 5;
    private List<GameObject> myPool = new List<GameObject>();

    protected override void Start()
    {
        base.Start();

        // Inicializar Pool
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
        while (true)
        {
            if (player != null)
            {
                // Mirar al jugador antes de disparar
                Vector3 dir = (player.position - transform.position).normalized;
                rb.MoveRotation(Quaternion.LookRotation(new Vector3(dir.x, 0, dir.z)));
                Shoot();
            }

            yield return new WaitForSeconds(1f);

            // Calcular nuevo destino aleatorio
            targetPosition = transform.position + new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
            isMoving = true;

            yield return new WaitForSeconds(fireRate);
            isMoving = false;
            rb.velocity = Vector3.zero; // Detenerse al llegar
        }
    }

    void FixedUpdate()
    {
        if (isMoving)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            direction.y = 0;

            Vector3 newPos = transform.position + direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);

            // Si llegamos muy cerca del destino, dejamos de movernos
            if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
            {
                isMoving = false;
            }
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
                return;
            }
        }
    }
}