using UnityEngine;

namespace KitchenInvasion {
    public class ShooterEnemy : Enemy {
        [SerializeField] float stopDistance = 2f;
        [SerializeField] float fireInterval = 2f;
        [SerializeField] float bulletSpeed = 2f;
        [SerializeField] Transform muzzle;

        ObjectPool bulletPool;
        float fireTimer = 1f;

        public void SetBulletPool(ObjectPool pool) { bulletPool = pool; }

        protected override void Move() { MoveTowardsPlayer(stopDistance); }

        protected override void Attack() {
            fireTimer -= Time.deltaTime;
            if (fireTimer > 0f || DistanceToPlayer() > stopDistance + 0.1f) return;
            fireTimer = fireInterval;
            Bullet bullet = bulletPool.Get();
            if (bullet == null) return;
            Vector3 direction = (player.transform.position - muzzle.position).normalized;
            bullet.Launch(muzzle.position, Quaternion.LookRotation(direction), direction * bulletSpeed);
            GameEvents.EnemyShot();
        }
    }
}
