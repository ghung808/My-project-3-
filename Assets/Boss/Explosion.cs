using UnityEngine;

public class Explosion : MonoBehaviour
{
    [Header("Explosion Settings")]
    public float radius = 1.5f;
    public int damage = 5;
    public float lifeTime = 0.5f;

    [Header("Layer")]
    public LayerMask soldierLayer;

    private bool exploded = false;

    void Start()
    {
        Explode();
        Destroy(gameObject, lifeTime);
    }

    void Explode()
    {
        if (exploded) return;
        exploded = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, soldierLayer);

        foreach (Collider2D hit in hits)
        {
            PlayerSoldier player = hit.GetComponent<PlayerSoldier>();
            if (player != null)
            {
                player.TakeDamage(damage);
                continue;
            }

            MageSoldier mage = hit.GetComponent<MageSoldier>();
            if (mage != null)
            {
                mage.TakeDamage(damage);
                continue;
            }

            ArcherSoldier archer = hit.GetComponent<ArcherSoldier>();
            if (archer != null)
            {
                archer.TakeDamage(damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}