using UnityEngine;

namespace KitchenInvasion {
    public abstract class Enemy : MonoBehaviour, IDamageable {
        [SerializeField] int maxHealth = 3;
        [SerializeField] float moveSpeed = 0.3f;
        [SerializeField] int scoreValue = 10;
        [SerializeField] float hitFlashTime = 0.18f;
        [SerializeField] Material flashMaterial;

        protected PlayerHealth player;
        protected float speedMultiplier = 1f;
        float groundHeight;
        int health;
        float hitTimer;
        Vector3 normalScale;
        Renderer[] renderers;
        Material[] normalMaterials;

        public int ScoreValue => scoreValue;

        public void Setup(PlayerHealth target, float speedBoost) {
            player = target;
            speedMultiplier = speedBoost;
            groundHeight = transform.position.y;
            health = maxHealth;
            normalScale = transform.localScale;
            renderers = GetComponentsInChildren<Renderer>();
            normalMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) normalMaterials[i] = renderers[i].sharedMaterial;
        }

        void Update() {
            if (player == null) return;
            Move();
            Attack();
            UpdateHitFeedback();
        }

        protected abstract void Move();
        protected abstract void Attack();

        protected Vector3 PlayerOnGround() {
            Vector3 target = player.transform.position;
            target.y = groundHeight;
            return target;
        }

        protected float DistanceToPlayer() { return Vector3.Distance(transform.position, PlayerOnGround()); }

        protected void MoveTowardsPlayer(float stopDistance) {
            Vector3 target = PlayerOnGround();
            if (DistanceToPlayer() > stopDistance) {
                float step = moveSpeed * speedMultiplier * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, target, step);
            }
            Vector3 lookDirection = target - transform.position;
            if (lookDirection.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(lookDirection);
        }

        public void TakeDamage(int amount) {
            health -= amount;
            hitTimer = hitFlashTime;
            SetFlash(true);
            GameEvents.EnemyHit();
            if (health <= 0) {
                GameEvents.EnemyKilled(this);
                Destroy(gameObject);
            }
        }

        void UpdateHitFeedback() {
            if (hitTimer <= 0f) return;
            hitTimer -= Time.deltaTime;
            float punch = Mathf.Clamp01(hitTimer / hitFlashTime);
            transform.localScale = normalScale * (1f + 0.35f * punch);
            if (hitTimer <= 0f) {
                transform.localScale = normalScale;
                SetFlash(false);
            }
        }

        void SetFlash(bool on) {
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].sharedMaterial = on ? flashMaterial : normalMaterials[i];
        }
    }
}
