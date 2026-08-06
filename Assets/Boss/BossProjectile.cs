using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 8f;
    public int damage = 5;
    public float lifeTime = 5f;

    [Header("Effects")]
    public GameObject hitEffect;
    public GameObject explosionPrefab;

    private Transform target;

    public void Initialize(Transform newTarget, int newDamage)
    {
        target = newTarget;
        damage = newDamage;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void HitTarget()
    {
        if (target != null)
        {
            PlayerSoldier player = target.GetComponent<PlayerSoldier>();
            MageSoldier mage = target.GetComponent<MageSoldier>();
            ArcherSoldier archer = target.GetComponent<ArcherSoldier>();

            if (player != null)
                player.TakeDamage(damage);

            if (mage != null)
                mage.TakeDamage(damage);

            if (archer != null)
                archer.TakeDamage(damage);
        }

        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Soldier"))
        {
            HitTarget();
        }
    }
}