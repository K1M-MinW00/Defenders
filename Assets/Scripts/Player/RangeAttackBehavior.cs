using UnityEngine;

public class RangedAttackBehavior : MonoBehaviour, IAttackBehavior
{
    [Header("Combat")]
    public float attackRange = 4f;
    public float attackCooldown = 0.5f;
    public float damage = 10f;

    [Header("Bullet")]
    public Bullet bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    private float lastAttackTime;
    private AmmoHandler ammoHandler;

    private void Awake()
    {
        ammoHandler = GetComponent<AmmoHandler>();
    }

    public void TryAttack(Transform target)
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        if (!ammoHandler.CanShoot())
        {
            StartCoroutine(ammoHandler.Reload());
            return;
        }

        Fire(target);
        ammoHandler.ConsumeAmmo();
        lastAttackTime = Time.time;
    }

    private void Fire(Transform target)
    {
        Vector2 dir = (target.position - firePoint.position).normalized;

        Bullet bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        bullet.Init(dir, bulletSpeed, damage);

        Debug.Log("ÃÑ¾Ë ¹ß»ç");
    }
}
