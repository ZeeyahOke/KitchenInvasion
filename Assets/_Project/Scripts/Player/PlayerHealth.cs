using UnityEngine;

namespace KitchenInvasion {
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerHealth : MonoBehaviour, IDamageable {
        int maxHealth = 100;
        int currentHealth;

        public bool IsDead => currentHealth <= 0;

        public void ResetHealth(int newMaxHealth) {
            maxHealth = newMaxHealth;
            currentHealth = maxHealth;
            GameEvents.HealthChanged(currentHealth, maxHealth);
        }

        public void TakeDamage(int amount) {
            if (IsDead || !GameManager.Instance.IsPlaying) return;
            currentHealth = Mathf.Max(currentHealth - amount, 0);
            GameEvents.HealthChanged(currentHealth, maxHealth);
            GameEvents.PlayerDamaged();
            if (IsDead) GameEvents.PlayerDied();
        }
    }
}
