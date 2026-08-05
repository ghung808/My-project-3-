using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Transform target;
    private int damage;
    public float speed = 10f;

    public void Seek(Transform _target, int _damage)
    {
        target = _target;
        damage = _damage;
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
    }

    void HitTarget()
    {
        if (target != null)
        {
            PlayerSoldier w = target.GetComponent<PlayerSoldier>();
            MageSoldier m = target.GetComponent<MageSoldier>();
            ArcherSoldier a = target.GetComponent<ArcherSoldier>();

            if (w != null) w.TakeDamage(damage);
            else if (m != null) m.TakeDamage(damage);
            else if (a != null) a.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}