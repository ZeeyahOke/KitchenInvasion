using UnityEngine;

namespace KitchenInvasion {
    public class Bullet : MonoBehaviour {
        [SerializeField] int damage = 1;
        [SerializeField] float lifetime = 2f;
        [SerializeField] bool firedByPlayer = true;

        ObjectPool pool;
        Vector3 velocity;
        float lifeTimer;

        public void SetPool(ObjectPool ownerPool) { pool = ownerPool; }

        public void Launch(Vector3 position, Quaternion rotation, Vector3 startVelocity) {
            transform.SetPositionAndRotation(position, rotation);
            velocity = startVelocity;
            lifeTimer = lifetime;
            gameObject.SetActive(true);
        }

        void Update() {
            transform.position += velocity * Time.deltaTime;
            lifeTimer -= Time.deltaTime;
            if (lifeTimer <= 0f) pool.Return(this);
        }

        void OnTriggerEnter(Collider other) {
            IDamageable target = other.GetComponentInParent<IDamageable>();
            if (target == null) return;
            bool hitPlayer = target is PlayerHealth;
            if (hitPlayer == firedByPlayer) return;
            target.TakeDamage(damage);
            pool.Return(this);
        }
    }
}
