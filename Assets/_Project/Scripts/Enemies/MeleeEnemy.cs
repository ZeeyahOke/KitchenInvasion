using UnityEngine;

namespace KitchenInvasion {
    public class MeleeEnemy : Enemy {
        [SerializeField] float attackRange = 0.5f;
        [SerializeField] float attackCooldown = 1.5f;
        [SerializeField] int attackDamage = 5;

        float attackTimer;

        protected override void Move() { MoveTowardsPlayer(attackRange * 0.8f); }

        protected override void Attack() {
            attackTimer -= Time.deltaTime;
            if (attackTimer > 0f || DistanceToPlayer() > attackRange) return;
            player.TakeDamage(attackDamage);
            GameEvents.MeleeHit();
            attackTimer = attackCooldown;
        }
    }
}
